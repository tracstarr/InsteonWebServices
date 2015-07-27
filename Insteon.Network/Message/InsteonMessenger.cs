using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Insteon.Network.Commands;
using Insteon.Network.Device;
using Insteon.Network.Enum;
using Insteon.Network.Helpers;
using ServiceStack.Logging;

namespace Insteon.Network.Message
{
    // This class is responsible for processing raw messages into structured property lists and dispatching the result to individual device objects.
    // The responsibilities of the messenger include:
    //  - Owning the network bridge to the physical INSTEON network.
    //  - Providing the ability to send messages to the controller for other classes in the module.
    //  - Processing raw message bytes into structured property lists.
    //  - Determining the logical device object to which the message is directed and dispatching the message to that object.
    //  - Reporting back to the bridge whether or not each message is valid, and if valid the size in bytes of the message.
    internal class InsteonMessenger : IMessageProcessor
    {
        private ILog logger = LogManager.GetLogger(typeof(InsteonMessenger));
        private readonly InsteonNetworkBridge bridge;
        private readonly Dictionary<string, Timer> duplicates = new Dictionary<string, Timer>(); // used to detect duplicate messages
        private readonly InsteonNetwork network;
        private readonly List<WaitItem> waitList = new List<WaitItem>();
        private bool echoCommand;
        private InsteonMessage echoMessage;
        private byte[] sentMessage; // bytes of last sent message, used to match the echo

        public InsteonMessenger(InsteonNetwork network)
        {
            if (network == null)
            {
                throw new ArgumentNullException("network");
            }

            this.network = network;
            bridge = new InsteonNetworkBridge(this);
            ControllerProperties = new Dictionary<PropertyKey, int>();
        }

        public Dictionary<PropertyKey, int> ControllerProperties { get; private set; }

        public bool IsConnected
        {
            get { return bridge.IsConnected; }
        }

        public void Close()
        {
            lock (bridge)
            {
                bridge.Close();
            }
            network.Disconnect();
        }

        public void Connect(InsteonConnection connection)
        {
            lock (bridge)
            {
                ControllerProperties = bridge.Connect(connection);
            }
            logger.DebugFormat("Connected to '{0}'", connection);

            // disable deadman 0x48 ? TODO: according to spec this should be 00010000 0x10?
            byte[] message = { (byte)InsteonModemSerialCommand.SetConfiguration, (byte)InsteonModemConfigurationFlags.DisableDeadman };
            Send(message);
        }

        private void DuplicateMessageTimerCallback(object state)
        {
            string key = state as string;
            lock (duplicates)
                if (duplicates.ContainsKey(key))
                {
                    duplicates.Remove(key);
                }
        }

        private bool IsDuplicateMessage(InsteonMessage message)
        {
            lock (duplicates)
            {
                // determine if message key matches an entry in the list
                foreach (var item in duplicates)
                {
                    if (message.Key == item.Key)
                    {
                        return true;
                    }
                }

                // create a new duplicte entry
                Timer timer = new Timer(DuplicateMessageTimerCallback, message.Key, 0, 1000);
                duplicates.Add(message.Key, timer);

                return false;
            }
        }

        private void OnMessage(InsteonMessage message)
        {
            if (message.Properties.ContainsKey(PropertyKey.FromAddress))
            {
                int address = message.Properties[PropertyKey.FromAddress];
                if (network.Devices.ContainsKey(address))
                {
                    logger.DebugFormat("Device {0} received message {1}", InsteonAddress.Format(address), message.ToString());
                    InsteonDevice device = network.Devices.Find(address);
                    device.OnMessage(message);
                }
                else if (message.MessageType == InsteonMessageType.SetButtonPressed)
                {
                    // don't warn about SetButtonPressed message from unknown devices, because it may be from a device about to be added
                }
                else if (network.AutoAdd)
                {
                    logger.DebugFormat("Unknown device {0} received message {1}, adding device", InsteonAddress.Format(address), message.ToString());

                    //note: due to how messages are handled and how devices cannot receive new messages while pending sends (I think) we should only add on certain message types.
                    // right now I've only tested devices where we get broadcast messages. Thus, we wait until the last message received. 
                    if (message.MessageType == InsteonMessageType.SuccessBroadcast)
                    {
                        InsteonIdentity? id;

                        // TODO: probably shouldn't be in a while loop. Need a better way to address this
                        while (!network.Controller.TryGetLinkIdentity(new InsteonAddress(address), out id))
                        {
                            if (id != null)
                            {
                                InsteonDevice device = network.Devices.Add(new InsteonAddress(address), id.Value);
                                device.OnMessage(message);
                            }
                        }
                    }
                }
                else
                {
                    logger.WarnFormat("Unknown device {0} received message {1}. Could be Identification process.", InsteonAddress.Format(address), message.ToString());
                }
            }
            else
            {
                logger.DebugFormat("Controller received message {0}", message.ToString());
                network.Controller.OnMessage(message);
            }
        }

        public void Send(byte[] message)
        {
            if (TrySend(message, true) != EchoStatus.ACK)
            {
                throw new IOException(string.Format("Failed to send message '{0}'", Utilities.ByteArrayToString(message)));
            }
        }

        public void SendReceive(byte[] message, byte receiveMessageId, out Dictionary<PropertyKey, int> properties)
        {
            if (TrySendReceive(message, true, receiveMessageId, null, out properties) != EchoStatus.ACK)
            {
                throw new IOException(string.Format("Failed to send message '{0}'.", Utilities.ByteArrayToString(message)));
            }
        }

        public bool TryConnect(InsteonConnection connection)
        {
            if (connection != null)
            {
                try
                {
                    logger.DebugFormat("Trying connection '{0}'...", connection.ToString());

                    lock (bridge)
                    {
                        ControllerProperties = bridge.Connect(connection);
                    }
                    logger.DebugFormat("Connected to '{0}'", connection);

                    // disable deadman 0x48 ? TODO: according to spec this should be 00010000 0x10?
                    byte[] message = { (byte)InsteonModemSerialCommand.SetConfiguration, (byte)InsteonModemConfigurationFlags.DisableDeadman };
                    TrySend(message);

                    return true;
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Could not connect to '{0}'. {1}", connection.ToString(), ex.Message);
                }
            }
            return false;
        }

        public EchoStatus TrySend(byte[] message, bool retryOnNak = true)
        {
            return TrySend(message, retryOnNak, message.Length);
        }

        public EchoStatus TrySend(byte[] message, bool retryOnNak, int echoLength)
        {
            EchoStatus status = EchoStatus.None;

            lock (bridge)
            {
                sentMessage = message;
                try
                {
                    status = bridge.Send(message, retryOnNak, echoLength);
                }
                catch (InvalidOperationException)
                {
                    logger.ErrorFormat("Bridge send command fatal error");
                }
                catch (IOException)
                {
                    logger.ErrorFormat("Bridge send command fatal error");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Unexpected failure... {0}", ex.Message);
                    if (Debugger.IsAttached)
                    {
                        throw;
                    }
                }
                finally
                {
                    sentMessage = null;
                }
            }

            if (status == EchoStatus.None)
            {
                logger.ErrorFormat("No response from serial port");
                network.OnDisconnected();
            }

            return status;
        }

        public EchoStatus TrySendEchoCommand(byte[] message, bool retryOnNak, int echoLength, out Dictionary<PropertyKey, int> properties)
        {
            echoMessage = null;

            echoCommand = true;
            EchoStatus status = TrySend(message, retryOnNak, echoLength);
            echoCommand = false;

            properties = echoMessage != null ? echoMessage.Properties : null;
            echoMessage = null;
            return status;
        }

        public EchoStatus TrySendReceive(byte[] message, bool retryOnNak, byte receiveMessageId, InsteonMessageType? receiveMessageType, out Dictionary<PropertyKey, int> properties)
        {
            properties = null;
            WaitItem item = new WaitItem(receiveMessageId, receiveMessageType);

            lock (waitList)
                waitList.Add(item);

            EchoStatus status = TrySend(message, retryOnNak);
            if (status == EchoStatus.ACK)
            {
                if (item.Message == null)
                {
                    item.MessageEvent.WaitOne(Constants.sendReceiveTimeout);
                }
                if (item.Message != null)
                {
                    properties = item.Message.Properties;
                }
                else
                {
                    logger.ErrorFormat("Did not receive expected message reply; SentMessage='{0}', ExpectedReceiveMessageId={1:X2}, Timeout={2}ms", Utilities.ByteArrayToString(message), receiveMessageId, Constants.sendReceiveTimeout);
                }
            }

            lock (waitList)
                waitList.Remove(item);

            return status;
        }

        private void UpdateWaitItems(InsteonMessage message)
        {
            lock (waitList)
            {
                for (int i = 0; i < waitList.Count; ++i)
                {
                    WaitItem item = waitList[i];

                    if (message.MessageId == item.MessageId)
                    {
                        if (item.MessageType == null || item.MessageType.Value == message.MessageType)
                        {
                            if (item.Message == null)
                            {
                                item.Message = message;
                                item.MessageEvent.Set();
                            }
                        }
                    }
                }
            }
        }

        public bool VerifyConnection()
        {
            if (!bridge.IsConnected)
            {
                return false;
            }

            byte[] message = { (byte)InsteonModemSerialCommand.GetImInfo };
            Dictionary<PropertyKey, int> properties;
            EchoStatus status = TrySendEchoCommand(message, true, 7, out properties);
            if (status == EchoStatus.ACK || status == EchoStatus.NAK)
            {
                return true;
            }

            logger.ErrorFormat("Verify connection failed");
            network.OnDisconnected();
            return false;
        }

        bool IMessageProcessor.ProcessMessage(byte[] data, int offset, out int count)
        {
            InsteonMessage message;
            if (InsteonMessageProcessor.ProcessMessage(data, offset, out count, out message))
            {
                if (!IsDuplicateMessage(message))
                {
                    //logger.DebugFormat("PROCESSOR: Message '{0}' processed...\r\n{1}", Utilities.ByteArrayToString(data, offset, count), message.ToString("Log"));
                    logger.DebugFormat("PROCESSOR: Message '{0}' processed...", Utilities.ByteArrayToString(data, offset, count));
                    OnMessage(message);
                    UpdateWaitItems(message);
                }
                else
                {
                    //logger.DebugFormat("PROCESSOR: Message '{0}' duplicate ignored...\r\n{1}", Utilities.ByteArrayToString(data, offset, count), message.ToString("Log"));
                    logger.DebugFormat("PROCESSOR: Message '{0}' duplicate ignored...", Utilities.ByteArrayToString(data, offset, count));
                }
                return true;
            }
            return false;
        }

        bool IMessageProcessor.ProcessEcho(byte[] data, int offset, out int count)
        {
            var message = Utilities.ArraySubset(data, offset, sentMessage.Length);
            if (echoCommand)
            {
                if (InsteonMessageProcessor.ProcessMessage(data, offset, out count, out echoMessage))
                {
                    //logger.DebugFormat("PROCESSOR: Echo '{0}' processed...\r\n{1}", Utilities.ByteArrayToString(data, offset, count), echoMessage.ToString("Log"));
                    logger.DebugFormat("PROCESSOR: Echo '{0}' processed...", Utilities.ByteArrayToString(data, offset, count));
                    return true;
                }
                return false;
            }
            if (Utilities.ArraySequenceEquals(sentMessage, message))
            {
                count = sentMessage.Length;
                logger.DebugFormat("PROCESSOR: Echo '{0}' matched", Utilities.ByteArrayToString(data, offset, count));
                return true;
            }
            count = 0;
            return false;
        }

        void IMessageProcessor.SetEchoStatus(EchoStatus status) { }

        private class WaitItem
        {
            public WaitItem(byte messageId, InsteonMessageType? messageType)
            {
                MessageId = messageId;
                MessageType = messageType;
                MessageEvent = new AutoResetEvent(false);
                Message = null;
            }

            public byte MessageId { get; private set; }
            public InsteonMessageType? MessageType { get; private set; }
            public AutoResetEvent MessageEvent { get; private set; }
            public InsteonMessage Message { get; set; }
        }
    }
}