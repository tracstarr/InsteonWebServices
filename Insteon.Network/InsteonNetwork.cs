using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Text;
using Insteon.Network.Device;
using Insteon.Network.Enum;
using Insteon.Network.Helpers;
using Insteon.Network.Message;
using ServiceStack.Logging;

namespace Insteon.Network
{
    /// <summary>
    /// Represents the top level INSTEON network, including the set of known INSTEON devices and the controller device.
    /// </summary>
    public class InsteonNetwork
    {
        private ILog logger = LogManager.GetLogger(typeof(InsteonNetwork));
        private List<InsteonConnection> connections;

        /// <summary>
        /// Initializes a new instance of the INSTEON network class.
        /// </summary>
        public InsteonNetwork()
        {
            Devices = new InsteonDeviceList(this);
            Messenger = new InsteonMessenger(this);
        }

        internal InsteonMessenger Messenger { get; private set; }

        /// <summary>
        /// A collection of known INSTEON devices linked to the network.
        /// </summary>
        public InsteonDeviceList Devices { get; private set; }

        /// <summary>
        /// The INSTEON controller device which interfaces to the various other INSTEON devices on the network.
        /// </summary>
        public InsteonController Controller { get; private set; }

        /// <summary>
        /// Determines whether devices are automatically added to the device collection when a message is received from a device not already in the device collection.
        /// </summary>
        public bool AutoAdd { get; set; }

        /// <summary>
        /// Returns the INSTEON network connection object, or null if the network is not connected. This object can be used later to reconnect to the same network.
        /// </summary>
        public InsteonConnection Connection { get; private set; }

        ///<summary>
        /// Determines whether the connection to the INSTEON network is active.
        /// </summary>
        public bool IsConnected
        {
            get { return Connection != null; }
        }

        /// <summary>
        /// Provides the last connect status result.
        /// </summary>
        public ConnectProgressChangedEventArgs LastConnectStatus { get; private set; }

        /// <summary>
        /// Invoked when a connection to an INSTEON network is established.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Communicates progress status during the sometimes lengthy process of connecting to a network.
        /// </summary>
        public event ConnectProgressChangedEventHandler ConnectProgress;

        /// <summary>
        /// Invoked when the INSTEON network is shutting down.
        /// </summary>
        public event EventHandler Closing;

        /// <summary>
        /// Invoked when the connection to an INSTEON network is interrupted.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// Connects to an INSTEON network using the specified connection.
        /// </summary>
        /// <param name="connection">Specifies the connection to the INSTEON powerline controller device, which can accessed serially or over the network. Examples: "serial: COM1" or "net: 192.168.2.5".</param>
        public void Connect(InsteonConnection connection)
        {
            Messenger.Connect(connection);
            Connection = connection;
            Controller = new InsteonController(this);
            OnConnected();
            LastConnectStatus = null;
        }

        /// <summary>
        /// Disconnects from the active INSTEON network and closes all open connections.
        /// </summary>
        /// <remarks>
        /// This method does not throw an exception.
        /// </remarks>
        public void Close()
        {
            logger.DebugFormat("Closing INSTEON network...");
            OnClosing();
            Messenger.Close();
            Connection = null;
            logger.DebugFormat("INSTEON network closed");
        }

        internal void Disconnect()
        {
            Connection = null;
            OnDisconnected();
        }

        /// <summary>
        /// Returns the available network and serial connections.
        /// </summary>
        /// <param name="refresh">Specifies whether to refresh the list. If called after TryConnectNet with false the list found by TryConnectNet will be returned.</param>
        /// <returns>An array of objects representing each available connection.</returns>
        /// <remarks>
        /// This is a blocking operation that accesses the local network.
        /// Through the progress event, connection status is reported and the operation can be cancelled.
        /// This method does not throw an exception.
        /// </remarks>
        public InsteonConnection[] GetAvailableConnections(bool refresh)
        {
            var list = new List<InsteonConnection>();
            var networkConnections = GetAvailableNetworkConnections(refresh);
            if (networkConnections != null)
            {
                list.AddRange(networkConnections);
            }
            var serialConnections = GetAvailableSerialConnections();
            list.AddRange(serialConnections);
            return list.ToArray();
        }

        /// <summary>
        /// Returns the available network connections.
        /// </summary>
        /// <param name="refresh">Specifies whether to refresh the list. If called after TryConnectNet with false the list found by TryConnectNet will be returned.</param>
        /// <returns>An array of objects representing each available network connection.</returns>
        /// <remarks>
        /// This is a blocking operation that accesses the local network.
        /// Through the progress event, connection status is reported and the operation can be cancelled.
        /// This method does not throw an exception.
        /// </remarks>
        public InsteonConnection[] GetAvailableNetworkConnections(bool refresh)
        {
            if (connections == null || refresh)
            {
                connections = new List<InsteonConnection>();

                OnConnectProgress(5, "Retrieving list from smartlinc.smarthome.com..."); // 5% progress
                if (LastConnectStatus.Cancel)
                {
                    connections = null;
                    return null;
                }

                var list = new List<SmartLincInfo>();
                list.AddRange(SmartLincFinder.GetRegisteredSmartLincs());
                foreach (SmartLincInfo item in list)
                {
                    OnConnectProgress(40*list.IndexOf(item)/list.Count + 10, string.Format("Accessing SmartLinc {0} of {1} at {2}", list.IndexOf(item) + 1, list.Count, item.Uri.Host)); // 10% to 50% progress
                    if (LastConnectStatus.Cancel)
                    {
                        connections = null;
                        return null;
                    }

                    string name = SmartLincFinder.GetSmartLincName(item.Uri.AbsoluteUri);
                    connections.Add(new InsteonConnection(InsteonConnectionType.Net, item.Uri.Host, name, item.InsteonAddress));
                }
            }
            return connections.ToArray();
        }

        /// <summary>
        /// Returns the available serial connections.
        /// </summary>
        /// <returns>An array of objects representing each available serial connection.</returns>
        /// <remarks>
        /// This method does not throw an exception.
        /// </remarks>
        public InsteonConnection[] GetAvailableSerialConnections()
        {
            var list = new List<InsteonConnection>();
            string[] ports = null;
            try
            {
                ports = SerialPort.GetPortNames();
            }
            catch (Win32Exception) {}
            if (ports != null)
            {
                Array.Sort(ports);
                foreach (string port in ports)
                {
                    list.Add(new InsteonConnection(InsteonConnectionType.Serial, port));
                }
            }
            return list.ToArray();
        }

        private void OnConnected()
        {
            if (Connected != null)
            {
                Connected(this, EventArgs.Empty);
            }
        }

        private void OnConnectProgress(int progressPercentage, string status)
        {
            LastConnectStatus = new ConnectProgressChangedEventArgs(progressPercentage, status);
            if (ConnectProgress != null)
            {
                ConnectProgress(this, LastConnectStatus);
            }
        }

        internal void OnDisconnected()
        {
            if (Disconnected != null)
            {
                Disconnected(this, EventArgs.Empty);
            }
        }

        private void OnClosing()
        {
            if (Closing != null)
            {
                Closing(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Attempts to locate an INSTEON controller by first searching the Smarthome web service to connect over the network, and then by searching the serial ports for a compatible controller.
        /// The first successful connection will be returned.
        /// </summary>
        /// <returns>Returns true if a connection was successfully made, or false if unable to find a connection.</returns>
        /// <remarks>
        /// This is a blocking operation that accesses the local network.
        /// Through the progress event, connection status is reported and the operation can be cancelled.
        /// Some devices such as the SmartLinc self-register with the Smarthome web service each time they are powered up.
        /// This method does not throw an exception.
        /// </remarks>
        public bool TryConnect()
        {
            var connections = GetAvailableConnections(true);
            if (LastConnectStatus.Cancel || connections == null || connections.Length <= 0)
            {
                return false;
            }

            foreach (InsteonConnection connection in connections)
            {
                logger.DebugFormat("Available connection '{0}'", connection.ToString());
            }

            return TryConnect(connections);
        }

        /// <summary>
        /// Connects to an INSTEON network using the specified connection.
        /// </summary>
        /// <param name="connection">Specifies the connection to the INSTEON controller device, which can accessed serially or over the network. Examples: "serial: COM1" or "net: 192.168.2.5".</param>
        /// <returns>Returns true if a connection was successfully made, or false if unable to find a connection.</returns>
        /// <remarks>
        /// This method does not throw an exception.
        /// </remarks>
        public bool TryConnect(InsteonConnection connection)
        {
            if (!Messenger.TryConnect(connection))
            {
                return false;
            }

            Connection = connection;
            Controller = new InsteonController(this);
            OnConnected();
            LastConnectStatus = null;

            return true;
        }

        /// <summary>
        /// Attempts to connect to an INSTEON network by trying each specified connection. The first successful connection will be returned.
        /// </summary>
        /// <param name="connections">Specifies the list of connections.</param>
        /// <returns>Returns true if a connection was successfully made, or false if unable to find a connection.</returns>
        /// <remarks>
        /// This is a blocking operation that accesses the local network.
        /// Through the progress event, connection status is reported and the operation can be cancelled.
        /// This method does not throw an exception.
        /// </remarks>
        public bool TryConnect(InsteonConnection[] connections)
        {
            if (connections != null)
            {
                var list = new List<InsteonConnection>();
                list.AddRange(connections);
                foreach (InsteonConnection connection in connections)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("Trying connection {0} of {1} on {2}", list.IndexOf(connection) + 1, list.Count, connection.Value);
                    if (connection.Name != connection.Value)
                    {
                        sb.AppendFormat(" '{0}'", connection.Name);
                    }
                    if (!connection.Address.IsEmpty)
                    {
                        sb.AppendFormat("  ({0})", connection.Address);
                    }

                    OnConnectProgress(50*list.IndexOf(connection)/list.Count + 50, sb.ToString()); // 50% to 100% progress
                    if (LastConnectStatus.Cancel)
                    {
                        return false;
                    }

                    if (TryConnect(connection))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Verifies that the connection to the INSTEON network is active.
        /// </summary>
        /// <returns>Returns true if the connection is verified.</returns>
        /// <remarks>
        /// If the verification fails the <see cref="Disconnected">Disconnected</see> event will be invoked and false will be returned.
        /// This method does not throw an exception.
        /// </remarks>
        public bool VerifyConnection()
        {
            return Messenger.VerifyConnection();
        }
    }
}