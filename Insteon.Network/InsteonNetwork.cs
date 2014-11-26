// <copyright company="INSTEON">
// Copyright (c) 2012 All Right Reserved, http://www.insteon.net
//
// This source is subject to the Common Development and Distribution License (CDDL). 
// Please see the LICENSE.txt file for more information.
// All other rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// </copyright>
// <author>Dave Templin</author>
// <email>info@insteon.net</email>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace Insteon.Network
{
    /// <summary>
    /// Represents the top level INSTEON network, including the set of known INSTEON devices and the controller device.
    /// </summary>
    public class InsteonNetwork
    {
        internal InsteonMessenger Messenger { get; private set; }
        private List<InsteonConnection> connections = null;

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
        /// A collection of known INSTEON devices linked to the network.
        /// </summary>
        public InsteonDeviceList Devices { get; private set; }
        
        /// <summary>
        /// The INSTEON controller device which interfaces to the various other INSTEON devices on the network.
        /// </summary>
        public InsteonController Controller { get; private set; }

        /// <summary>
        /// Initializes a new instance of the INSTEON network class.
        /// </summary>
        public InsteonNetwork()
        {
            Devices = new InsteonDeviceList(this);
            Messenger = new InsteonMessenger(this);
        }

        /// <summary>
        /// Determines whether devices are automatically added to the device collection when a message is received from a device not already in the device collection.
        /// </summary>
        public bool AutoAdd { get; set; }

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
            Log.WriteLine("Closing INSTEON network...");
            OnClosing();
            Messenger.Close();
            Connection = null;
            Log.WriteLine("INSTEON network closed");
        }

        /// <summary>
        /// Returns the INSTEON network connection object, or null if the network is not connected. This object can be used later to reconnect to the same network.
        /// </summary>
        public InsteonConnection Connection { get; private set; }

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
            List<InsteonConnection> list = new List<InsteonConnection>();
            InsteonConnection[] networkConnections = GetAvailableNetworkConnections(refresh);
            if (networkConnections != null)
                list.AddRange(networkConnections);
            InsteonConnection[] serialConnections = GetAvailableSerialConnections();
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
                
                List<SmartLincInfo> list = new List<SmartLincInfo>();
                list.AddRange(SmartLincFinder.GetRegisteredSmartLincs());
                foreach (SmartLincInfo item in list)
                {
                    OnConnectProgress(40 * list.IndexOf(item) / list.Count + 10, string.Format("Accessing SmartLinc {0} of {1} at {2}", list.IndexOf(item) + 1, list.Count, item.Uri.Host));  // 10% to 50% progress
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
            List<InsteonConnection> list = new List<InsteonConnection>();
            string[] ports = null;
            try
            {
                ports = SerialPort.GetPortNames();
            }
            catch (Win32Exception)
            {
            }
            if (ports != null)
            {
                Array.Sort(ports);
                foreach (string port in ports)
                    list.Add(new InsteonConnection(InsteonConnectionType.Serial, port));
            }
            return list.ToArray();
        }

        ///<summary>
        /// Determines whether the connection to the INSTEON network is active.
        /// </summary>
        public bool IsConnected { get { return Connection != null; } }

        /// <summary>
        /// Provides the last connect status result.
        /// </summary>
        public ConnectProgressChangedEventArgs LastConnectStatus { get; private set; }

        private void OnConnected()
        {
            if (Connected != null)
                Connected(this, EventArgs.Empty);
        }

        private void OnConnectProgress(int progressPercentage, string status)
        {
            LastConnectStatus = new ConnectProgressChangedEventArgs(progressPercentage, status);
            if (ConnectProgress != null)
                ConnectProgress(this, LastConnectStatus);
        }

        internal void OnDisconnected()
        {
            if (Disconnected != null)
                Disconnected(this, EventArgs.Empty);
        }

        private void OnClosing()
        {
            if (Closing != null)
                Closing(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the folder path where INSTEON log files will be written.
        /// </summary>
        /// <param name="path">Full path to the specified folder.</param>
        /// <remarks>Folder must exist or an error will occur.</remarks>
        public static void SetLogPath(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException();
            Log.Open(path);
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
            InsteonConnection[] connections = GetAvailableConnections(true);
            if (LastConnectStatus.Cancel || connections == null || connections.Length <= 0)
                return false;

            foreach (InsteonConnection connection in connections)                
                Log.WriteLine("Available connection '{0}'", connection.ToString());
            
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
                return false;

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
                List<InsteonConnection> list = new List<InsteonConnection>();
                list.AddRange(connections);
                foreach (InsteonConnection connection in connections)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("Trying connection {0} of {1} on {2}", list.IndexOf(connection) + 1, list.Count, connection.Value);
                    if (connection.Name != connection.Value)
                        sb.AppendFormat(" '{0}'", connection.Name);
                    if (!connection.Address.IsEmpty)
                        sb.AppendFormat("  ({0})", connection.Address.ToString());

                    OnConnectProgress(50 * list.IndexOf(connection) / list.Count + 50, sb.ToString()); // 50% to 100% progress
                    if (LastConnectStatus.Cancel)
                        return false;

                    if (TryConnect(connection))
                        return true;
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
