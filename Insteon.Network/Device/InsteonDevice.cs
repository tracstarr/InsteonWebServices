using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Insteon.Network.Commands;
using Insteon.Network.Enum;
using Insteon.Network.Helpers;
using Insteon.Network.Message;
using ServiceStack.Logging;

namespace Insteon.Network.Device
{
    /// <summary>
    /// Represents an individual INSTEON device on the network.
    /// </summary>
    public class InsteonDevice
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(InsteonDevice));
        private readonly Timer ackTimer; // timeout to receive ACK from device
        private readonly InsteonNetwork network;
        private readonly AutoResetEvent pendingEvent = new AutoResetEvent(false);

        private DimmerDirection dimmerDirection = DimmerDirection.None;
        private InsteonDirectCommands? pendingCommand; // Gets the command that is currently pending on the device, or null if no command is pending.
        private int pendingRetry; // retry count for pending command
        private byte pendingValue;

        internal InsteonDevice(InsteonNetwork network, InsteonAddress address, InsteonIdentity identity)
        {
            this.network = network;
            Address = address;
            Identity = identity;
            ackTimer = new Timer(PendingCommandTimerCallback, null, Timeout.Infinite, Constants.deviceAckTimeout);
        }
        
        public string DeviceName
        {
            get
            {
                return string.Format("{0} [{1}]", Identity.GetSubCategoryName(), Address);
            }
        }

        /// <summary>
        /// The INSTEON address of the device.
        /// </summary>
        public InsteonAddress Address { get; private set; }

        /// <summary>
        /// Indicates the type of the INSTEON device.
        /// </summary>
        public InsteonIdentity Identity { get; private set; }

        /// <summary>
        /// Invoked when a device fails to respond to a command within the timeout period of 2 seconds.
        /// </summary>
        public event InsteonDeviceEventHandler DeviceCommandTimeout;

        /// <summary>
        /// Invoked when the device has been identified.
        /// </summary>
        public event InsteonDeviceEventHandler DeviceIdentified;

        /// <summary>
        /// Invoked when a status message is received from the INSTEON device, for example when the device turns on or off.
        /// </summary>
        public event InsteonDeviceStatusChangedEventHandler DeviceStatusChanged;
        
        /// <summary>
        /// Sends an INSTEON command to the device.
        /// </summary>
        /// <remarks>
        /// This is a non-blocking method that sends an INSTEON message to the target device and returns immediately (as long as another command is not already pending for the device). 
        /// Only one command can be pending to an INSTEON device at a time. This method will block if a second command is sent while a first command is still pending.
        /// The <see cref="DeviceStatusChanged">DeviceStatusChanged</see> event will be invoked if the command is successful.
        /// The <see cref="DeviceCommandTimeout">DeviceCommandTimeout</see> event will be invoked if the device does not respond within the expected timeout period.
        /// </remarks>
        /// <param name="command">Specifies the INSTEON device command to be invoked.</param>
        /// <param name="value">A parameter value required by some commands.</param>
        internal void Command(InsteonDirectCommands command, byte value)
        {
            if (!TryCommand(command, value))
            {
                throw new IOException(string.Format("Failed to send command '{0}' for device '{1}'", command, Address));
            }
        }
        
        /// <summary>
        /// Gets a value that indicates the on-level of the device.
        /// </summary>
        /// <returns>
        /// A value indicating the on-level of the device. For a dimmer a value between 0 and 255 will be returned. For a non-dimmer a value 0 or 255 will be returned.
        /// </returns>
        /// <remarks>
        /// This is a blocking method that sends an INSTEON message to the target device and waits for a reply, or until the device command times out.
        /// </remarks>
        public byte GetOnLevel()
        {
            byte value;
            if (!TryGetOnLevel(out value))
            {
                throw new IOException();
            }
            return value;
        }

        private static byte[] GetStandardMessage(InsteonAddress address, byte cmd1, byte cmd2)
        {
            // 0x0F is "Message Flags" - type and hops
            byte[] message = { (byte)InsteonModemSerialCommand.StandardOrExtendedMessage, address[2], address[1], address[0], (byte) MessageFlagsStandard.ThreeHopsThreeRemaining, cmd1, cmd2 };
            return message;
        }


        //TODO: probably remove from here now?
        /// <summary>
        /// Determines the type of INSTEON device by querying the device.
        /// </summary>
        /// <remarks>
        /// This is a non-blocking method that sends an INSTEON message to the target device and returns immediately (as long as another command is not already pending for the device). 
        /// Only one command can be pending to an INSTEON device at a time. This method will block if a second command is sent while a first command is still pending.
        /// The <see cref="DeviceIdentified">DeviceIdentified</see> event will be invoked if the command is successful.
        /// The <see cref="DeviceCommandTimeout">DeviceCommandTimeout</see> event will be invoked if the device does not respond within the expected timeout period.
        /// </remarks>
        public void Identify()
        {
            Identity = new InsteonIdentity();
            Command(InsteonDirectCommands.IDRequest, Byte.MinValue);
        }

        private void OnDeviceCommandTimeout()
        {
            if (DeviceCommandTimeout != null)
            {
                DeviceCommandTimeout(this, new InsteonDeviceEventArgs(this));
            }
            network.Devices.OnDeviceCommandTimeout(this);
        }

        private void OnDeviceIdentified()
        {
            if (DeviceIdentified != null)
            {
                DeviceIdentified(this, new InsteonDeviceEventArgs(this));
            }
            network.Devices.OnDeviceIdentified(this);
        }

        private void OnDeviceStatusChanged(InsteonDeviceStatus status)
        {
            if (DeviceStatusChanged != null)
            {
                DeviceStatusChanged(this, new InsteonDeviceStatusChangedEventArgs(this, status));
            }
            network.Devices.OnDeviceStatusChanged(this, status);
        }

        private void OnSetButtonPressed(InsteonMessage message)
        {
            if (Identity.IsEmpty)
            {
                var devCat = (byte)message.Properties[PropertyKey.DevCat];
                var subCat = (byte)message.Properties[PropertyKey.SubCat];
                var firmwareVersion = (byte)message.Properties[PropertyKey.FirmwareVersion];
                Identity = new InsteonIdentity(devCat, subCat, firmwareVersion);
            }
            OnDeviceIdentified();
        }

        internal virtual void OnMessage(InsteonMessage message)
        {
            switch (message.MessageType)
            {
                case InsteonMessageType.Ack:
                    PendingCommandAck(message);
                    break;

                case InsteonMessageType.OnCleanup:
                    OnDeviceStatusChanged(InsteonDeviceStatus.On);
                    break;

                case InsteonMessageType.OffCleanup:
                    OnDeviceStatusChanged(InsteonDeviceStatus.Off);
                    break;

                case InsteonMessageType.FastOnCleanup:
                    OnDeviceStatusChanged(InsteonDeviceStatus.On);
                    OnDeviceStatusChanged(InsteonDeviceStatus.FastOn);
                    break;

                case InsteonMessageType.FastOffCleanup:
                    OnDeviceStatusChanged(InsteonDeviceStatus.Off);
                    OnDeviceStatusChanged(InsteonDeviceStatus.FastOff);
                    break;

                case InsteonMessageType.IncrementBeginBroadcast:
                    dimmerDirection = message.Properties[PropertyKey.IncrementDirection] != 0 ? DimmerDirection.Up : DimmerDirection.Down;
                    break;

                case InsteonMessageType.IncrementEndBroadcast:
                    if (dimmerDirection == DimmerDirection.Up)
                    {
                        OnDeviceStatusChanged(InsteonDeviceStatus.Brighten);
                    }
                    else if (dimmerDirection == DimmerDirection.Down)
                    {
                        OnDeviceStatusChanged(InsteonDeviceStatus.Dim);
                    }
                    break;

                case InsteonMessageType.SetButtonPressed:
                    OnSetButtonPressed(message);
                    break;
            }
        }

        // if a command is pending determines whether the current message completes the pending command
        private void PendingCommandAck(InsteonMessage message)
        {
            lock (pendingEvent)
            {
                if (pendingCommand != null)
                {
                    var cmd1 = message.Properties[PropertyKey.Cmd1];
                    if (System.Enum.IsDefined(typeof(InsteonDirectCommands), cmd1))
                    {
                        var command = (InsteonDirectCommands)cmd1;
                        if (pendingCommand.Value == command)
                        {
                            pendingCommand = null;
                            pendingValue = 0;
                            ackTimer.Change(Timeout.Infinite, Timeout.Infinite); // stop ACK timeout timer
                            pendingEvent.Set(); // unblock any thread that may be waiting on the pending command
                        }
                    }
                }
            }
        }

        private void ClearPendingCommand()
        {
            lock (pendingEvent)
            {
                pendingCommand = null;
                pendingValue = 0;
                ackTimer.Change(Timeout.Infinite, Timeout.Infinite); // stop ACK timeout timer
                pendingEvent.Set(); // unblock any thread that may be waiting on the pending command
            }
        }

        // invoked when a pending command times out
        private void PendingCommandTimerCallback(object state)
        {
            ackTimer.Change(Timeout.Infinite, Timeout.Infinite); // stop ACK timeout timer

            var retry = false;
            var command = InsteonDirectCommands.On;
            byte value = 0;
            var retryCount = 0;

            lock (pendingEvent)
            {
                if (pendingCommand == null)
                {
                    return;
                }

                pendingRetry += 1;
                if (pendingRetry <= Constants.deviceCommandRetries)
                {
                    retry = true;
                    value = pendingValue;
                    retryCount = pendingRetry;
                }
                else
                {
                    retry = false;
                    command = pendingCommand.Value;
                    pendingCommand = null;
                    pendingValue = 0;
                    pendingEvent.Set(); // unblock any thread that may be waiting on the pending command                
                }
            }

            if (retry)
            {
                logger.WarnFormat("Device {0} Command {1} timed out, retry {2} of {3}...", Address.ToString(), command, retryCount, Constants.deviceCommandRetries);
                TryCommandInternal(command, value);
            }
            else
            {
                logger.ErrorFormat("Device {0} Command {1} timed out", Address.ToString(), command);
                OnDeviceCommandTimeout();
            }
        }
        
        /// <summary>
        /// Sends an INSTEON command to the device.
        /// </summary>
        /// <param name="command">Specifies the INSTEON device command to be invoked.</param>
        /// <param name="value">A parameter value required by some commands.</param>
        /// <remarks>
        /// This method does not throw an exception.
        /// This is a non-blocking method that sends an INSTEON message to the target device and returns immediately (as long as another command is not already pending for the device). 
        /// Only one command can be pending to an INSTEON device at a time. This method will block if a second command is sent while a first command is still pending.
        /// The <see cref="DeviceStatusChanged">DeviceStatusChanged</see> event will be invoked if the command is successful.
        /// The <see cref="DeviceCommandTimeout">DeviceCommandTimeout</see> event will be invoked if the device does not respond within the expected timeout period.
        /// </remarks>
        internal bool TryCommand(InsteonDirectCommands command, byte value)
        {
            WaitAndSetPendingCommand(command, value);
            return TryCommandInternal(command, value);
        }

        private bool TryCommandInternal(InsteonDirectCommands command, byte value)
        {
            var message = GetStandardMessage(Address, (byte)command, value);
            logger.DebugFormat("Device {0} Command(command:{1}, value:{2:X2})", Address.ToString(), command.ToString(), value);

            var status = network.Messenger.TrySend(message);
            if (status == EchoStatus.ACK)
            {
                ackTimer.Change(Constants.deviceAckTimeout, Timeout.Infinite); // start ACK timeout timer  
                ClearPendingCommand();
                return true;
            }
            ClearPendingCommand();
            return false;
        }

        /// <summary>
        /// Gets a value that indicates the on-level of the device.
        /// </summary>
        /// <returns>
        /// A value indicating the on-level of the device. For a dimmer a value between 0 and 255 will be returned. For a non-dimmer a value 0 or 255 will be returned.
        /// </returns>
        /// <remarks>
        /// This is a blocking method that sends an INSTEON message to the target device and waits for a reply, or until the device command times out.
        /// </remarks>
        public bool TryGetOnLevel(out byte value)
        {
            var command = InsteonDirectCommands.StatusRequest;
            WaitAndSetPendingCommand(command, 0);
            logger.DebugFormat("Device {0} GetOnLevel", Address.ToString());
            var message = GetStandardMessage(Address, (byte)command, 0);
            Dictionary<PropertyKey, int> properties;
            var status = network.Messenger.TrySendReceive(message, true, (byte) InsteonModemSerialCommand.StandardMessage, null, out properties); // on-level returned in cmd2 of ACK
            if (status == EchoStatus.ACK && properties != null)
            {
                value = (byte)properties[PropertyKey.Cmd2];
                logger.DebugFormat("Device {0} GetOnLevel returning {1:X2}", Address.ToString(), value);
                ClearPendingCommand();
                return true;
            }
            ClearPendingCommand();
            value = 0;
            return false;
        }

        /// <summary>
        /// Determines the type of INSTEON device by querying the device.
        /// </summary>
        /// <remarks>
        /// This method does not throw an exception.
        /// This is a non-blocking method that sends an INSTEON message to the target device and returns immediately (as long as another command is not already pending for the device). 
        /// Only one command can be pending to an INSTEON device at a time. This method will block if a second command is sent while a first command is still pending.
        /// The <see cref="DeviceIdentified">DeviceIdentified</see> event will be invoked if the command is successful.
        /// The <see cref="DeviceCommandTimeout">DeviceCommandTimeout</see> event will be invoked if the device does not respond within the expected timeout period.
        /// </remarks>
        public bool TryIdentify()
        {
            Identity = new InsteonIdentity();
            return TryCommand(InsteonDirectCommands.IDRequest, Byte.MinValue);
        }

        /// <summary>
        /// Removes links within both the INSTEON device and the INSTEON controller for the specified group.
        /// </summary>
        /// <param name="group">The specified group within which links are to be removed.</param>
        /// <remarks>
        /// This method does not throw an exception.
        /// This is a non-blocking method that sends an INSTEON message to the target device and returns immediately (as long as another command is not already pending for the device). 
        /// Only one command can be pending to an INSTEON device at a time. This method will block if a second command is sent while a first command is still pending.
        /// A <see cref="InsteonController.DeviceLinked">DeviceLinked</see> event will be invoked on the controller if the command is successful.
        /// The <see cref="DeviceCommandTimeout">DeviceCommandTimeout</see> event will be invoked if the device does not respond within the expected timeout period.
        /// </remarks>
        public bool TryUnlink(byte group)
        {
            if (network.Controller.TryEnterLinkMode(InsteonLinkMode.Delete, group))
            {
                return TryCommand(InsteonDirectCommands.EnterLinkingMode, group);
            }
            return false;
        }

        // blocks the current thread if a command is pending, then sets the current command as the pending command (note does not apply to all commands)
        private void WaitAndSetPendingCommand(InsteonDirectCommands command, byte value)
        {
            InsteonDirectCommands latchedPendingCommand;

            lock (pendingEvent)
            {
                if (pendingCommand == null)
                {
                    pendingCommand = command;
                    pendingValue = value;
                    pendingRetry = 0;
                    return;
                }
                latchedPendingCommand = pendingCommand.Value;
            }

            // block current thread if a command is pending
            logger.DebugFormat("Device {0} blocking command {1} for pending command {2}", Address.ToString(), command.ToString(), latchedPendingCommand.ToString());
            pendingEvent.Reset();
            if (!pendingEvent.WaitOne(Constants.deviceAckTimeout)) // wait at most deviceAckTimeout seconds
            {
                ClearPendingCommand(); // break deadlock and warn
                logger.WarnFormat("Device {0} unblocking command {1} for pending command {2}", Address.ToString(), command.ToString(), latchedPendingCommand.ToString());
            }

            WaitAndSetPendingCommand(command, value); // try again
        }

        /// <summary>
        /// Removes links within both the INSTEON device and the INSTEON controller for the specified group.
        /// </summary>
        /// <param name="group">The specified group within which links are to be removed.</param>
        /// <remarks>
        /// This method does not throw an exception.
        /// This is a non-blocking method that sends an INSTEON message to the target device and returns immediately (as long as another command is not already pending for the device). 
        /// Only one command can be pending to an INSTEON device at a time. This method will block if a second command is sent while a first command is still pending.
        /// A <see cref="InsteonController.DeviceLinked">DeviceLinked</see> event will be invoked on the controller if the command is successful.
        /// The <see cref="DeviceCommandTimeout">DeviceCommandTimeout</see> event will be invoked if the device does not respond within the expected timeout period.
        /// </remarks>
        public void Unlink(byte group)
        {
            network.Controller.EnterLinkMode(InsteonLinkMode.Delete, group);
            Command(InsteonDirectCommands.EnterUnlinkingMode, group);
        }

        public override string ToString()
        {
            return Identity.GetSubCategoryName();
            
        }
    }
}