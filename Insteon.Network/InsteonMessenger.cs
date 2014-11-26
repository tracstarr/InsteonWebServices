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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Insteon.Network
{
    // This class is responsible for processing raw messages into structured property lists and dispatching the result to individual device objects.
    // The responsibilities of the messenger include:
    //  - Owning the network bridge to the physical INSTEON network.
    //  - Providing the ability to send messages to the controller for other classes in the module.
    //  - Processing raw message bytes into structured property lists.
    //  - Determining the logical device object to which the message is directed and dispatching the message to that object.
    //  - Reporting back to the bridge whether or not each message is valid, and if valid the size in bytes of the message.
    internal class InsteonMessenger : InsteonNetworkBridge.IMessageProcessor
    {
        private readonly InsteonNetwork network;
        private readonly InsteonNetworkBridge bridge;
        private readonly List<WaitItem> waitList = new List<WaitItem>();
        private readonly Dictionary<string, Timer> duplicates = new Dictionary<string, Timer>(); // used to detect duplicate messages
        private byte[] sentMessage = null; // bytes of last sent message, used to match the echo
        private bool echoCommand = false;
        private InsteonMessage echoMessage = null;

        public Dictionary<PropertyKey, int> ControllerProperties { get; private set; }

        public bool IsConnected { get { return bridge.IsConnected; } }

        public InsteonMessenger(InsteonNetwork network)
        {
            if (network == null)
                throw new ArgumentNullException("network");

            this.network = network;
            bridge = new InsteonNetworkBridge(this);
            ControllerProperties = new Dictionary<PropertyKey, int>();
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
            Log.WriteLine("Connected to '{0}'", connection);

            byte[] message = { 0x6B, 0x48 }; // disable deadman
            Send(message);
        }

        private void DuplicateMessageTimerCallback(object state)
        {
            string key = state as string;
            lock (duplicates)
                if (duplicates.ContainsKey(key))
                    duplicates.Remove(key);
        }

        private bool IsDuplicateMessage(InsteonMessage message)
        {
            lock (duplicates)
            {
                // determine if message key matches an entry in the list
                foreach (KeyValuePair<string, Timer> item in duplicates)
                    if (message.Key == item.Key)
                        return true;

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
                    Log.WriteLine("Device {0} received message {1}", InsteonAddress.Format(address), message.ToString());
                    InsteonDevice device = network.Devices.Find(address);
                    device.OnMessage(message);
                }
                else if (message.MessageType == InsteonMessageType.SetButtonPressed)
                {
                    // don't warn about SetButtonPressed message from unknown devices, because it may be from a device about to be added
                }
                else if (network.AutoAdd)
                {
                    Log.WriteLine("Unknown device {0} received message {1}, adding device", InsteonAddress.Format(address), message.ToString());
                    InsteonDevice device = network.Devices.Add(new InsteonAddress(address), new InsteonIdentity());
                    device.OnMessage(message);
                }
                else
                {
                    Log.WriteLine("WARNING: Unknown device {0} received message {1}", InsteonAddress.Format(address), message.ToString());
                }
            }
            else
            {
                Log.WriteLine("Controller received message {0}", message.ToString());
                network.Controller.OnMessage(message);
            }
        }

        public void Send(byte[] message)
        {
            if (TrySend(message, true) != EchoStatus.ACK)
                throw new IOException(string.Format("Failed to send message '{0}'", Utilities.ByteArrayToString(message)));
        }

        public void SendReceive(byte[] message, byte receiveMessageId, out Dictionary<PropertyKey, int> properties)
        {
            if (TrySendReceive(message, true, receiveMessageId, out properties) != EchoStatus.ACK)
                throw new IOException(string.Format("Failed to send message '{0}'.", Utilities.ByteArrayToString(message)));
        }

        public bool TryConnect(InsteonConnection connection)
        {
            if (connection != null)
            {
                try
                {
                    Log.WriteLine("Trying connection '{0}'...", connection.ToString());

                    lock (bridge)
                    {
                        ControllerProperties = bridge.Connect(connection);
                    }
                    Log.WriteLine("Connected to '{0}'", connection);

                    byte[] message = { 0x6B, 0x48 }; // disable deadman
                    TrySend(message);

                    return true;
                }
                catch (Exception ex)
                {
                    Log.WriteLine("ERROR: Could not connect to '{0}'. {1}", connection.ToString(), ex.Message);
                }
            }
            return false;
        }

        public EchoStatus TrySend(byte[] message)
        {
            return TrySend(message, true);
        }

        public EchoStatus TrySend(byte[] message, bool retryOnNak)
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
                    Log.WriteLine("ERROR: Bridge send command fatal error");
                }
                catch (IOException)
                {
                    Log.WriteLine("ERROR: Bridge send command fatal error");
                }
                catch (Exception ex)
                {
                    Log.WriteLine("ERROR: Unexpected failure... {0}", ex.Message);
                    if (Debugger.IsAttached)
                        throw;
                }
                finally
                {
                    sentMessage = null;
                }
            }

            if (status == EchoStatus.None)
            {
                Log.WriteLine("ERROR: No response from serial port");
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

        public EchoStatus TrySendReceive(byte[] message, bool retryOnNak, byte receiveMessageId, out Dictionary<PropertyKey, int> properties)
        {
            properties = null;
            WaitItem item = new WaitItem(receiveMessageId);
            
            lock (waitList)
                waitList.Add(item);

            EchoStatus status = TrySend(message, retryOnNak);
            if (status == EchoStatus.ACK)
            {
                if (item.Message == null)
                    item.MessageEvent.WaitOne(Constants.sendReceiveTimeout);
                if (item.Message != null)
                    properties = item.Message.Properties;
                else
                    Log.WriteLine("ERROR: Did not receive expected message reply; SentMessage='{0}', ExpectedReceiveMessageId={1:X2}, Timeout={2}ms", Utilities.ByteArrayToString(message), receiveMessageId, Constants.sendReceiveTimeout);
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
                        if (item.Message == null)
                        {
                            item.Message = message;
                            item.MessageEvent.Set();
                        }
                }
            }
        }

        public bool VerifyConnection()
        {
            if (!bridge.IsConnected)
                return false;

            byte[] message = { 0x60 };
            Dictionary<PropertyKey, int> properties;
            EchoStatus status = TrySendEchoCommand(message, true, 7, out properties);
            if (status == EchoStatus.ACK || status == EchoStatus.NAK)
                return true;

            Log.WriteLine("ERROR: Verify connection failed");
            network.OnDisconnected();
            return false;
        }

        #region InsteonNetworkBridge.IMessageProcessor

        bool InsteonNetworkBridge.IMessageProcessor.ProcessMessage(byte[] data, int offset, out int count)
        {
            InsteonMessage message;
            if (InsteonMessageProcessor.ProcessMessage(data, offset, out count, out message))
            {
                if (!IsDuplicateMessage(message))
                {
                    Log.WriteLine("PROCESSOR: Message '{0}' processed...\r\n{1}", Utilities.ByteArrayToString(data, offset, count), message.ToString("Log"));
                    OnMessage(message);
                    UpdateWaitItems(message);
                }
                else
                {
                    Log.WriteLine("PROCESSOR: Message '{0}' duplicate ignored...\r\n{1}", Utilities.ByteArrayToString(data, offset, count), message.ToString("Log"));
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        bool InsteonNetworkBridge.IMessageProcessor.ProcessEcho(byte[] data, int offset, out int count)
        {
            byte[] message = Utilities.ArraySubset(data, offset, sentMessage.Length);
            if (echoCommand)
            {
                if (InsteonMessageProcessor.ProcessMessage(data, offset, out count, out echoMessage))
                {
                    Log.WriteLine("PROCESSOR: Echo '{0}' processed...\r\n{1}", Utilities.ByteArrayToString(data, offset, count), echoMessage.ToString("Log"));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Utilities.ArraySequenceEquals(sentMessage, message))
            {
                count = sentMessage.Length;
                Log.WriteLine("PROCESSOR: Echo '{0}' matched", Utilities.ByteArrayToString(data, offset, count));
                return true;
            }
            else
            {
                count = 0;
                return false;
            }
        }

        void InsteonNetworkBridge.IMessageProcessor.SetEchoStatus(EchoStatus status)
        {
        }

        #endregion

        private class WaitItem
        {
            public WaitItem(byte messageId)
            {
                this.MessageId = messageId;
                this.MessageEvent = new AutoResetEvent(false);
                this.Message = null;
            }
            public byte MessageId { get; private set; }
            public AutoResetEvent MessageEvent { get; private set; }
            public InsteonMessage Message { get; set; }
        }
    }
}
