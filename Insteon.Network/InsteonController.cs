using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Insteon.Network.Commands;
using Insteon.Network.Device;
using Insteon.Network.Enum;
using Insteon.Network.Message;
using ServiceStack.Logging;

namespace Insteon.Network
{
    /// <summary>
    /// Represents the controller device, which interfaces with the various other INSTEON devices on the network.
    /// </summary>
    public class InsteonController
    {
        private ILog logger = LogManager.GetLogger(typeof (InsteonController));
        private readonly InsteonNetwork network;
        private readonly Timer timer = new Timer();
        private InsteonLinkMode? linkingMode;

        internal InsteonController(InsteonNetwork network)
            : this(
                network,
                new InsteonAddress(network.Messenger.ControllerProperties[PropertyKey.Address]),
                new InsteonIdentity(
                    (byte)network.Messenger.ControllerProperties[PropertyKey.DevCat],
                    (byte)network.Messenger.ControllerProperties[PropertyKey.SubCat],
                    (byte)network.Messenger.ControllerProperties[PropertyKey.FirmwareVersion]
                    )
                ) { }

        private InsteonController(InsteonNetwork network, InsteonAddress address, InsteonIdentity identity)
        {
            this.network = network;
            Address = address;
            Identity = identity;

            timer.Interval = 4 * 60 * 1000; // 4 minutes
            timer.AutoReset = false;
            timer.Elapsed += (sender, args) =>
                {
                    IsInLinkingMode = false;
                    OnDeviceLinkTimeout();
                };
        }

        /// <summary>
        /// The INSTEON address of the controller device.
        /// </summary>
        public InsteonAddress Address { get; private set; }

        /// <summary>
        /// Indicates the type of the INSTEON device.
        /// </summary>
        public InsteonIdentity Identity { get; private set; }

        /// <summary>
        /// Determines whether the controller is in linking mode.
        /// </summary>
        public bool IsInLinkingMode { get; private set; }

        /// <summary>
        /// Invoked when an INSTEON device is linked to the controller device.
        /// </summary>
        public event InsteonDeviceEventHandler DeviceLinked;

        /// <summary>
        /// Invoked when an initiated link operation has timed out after 4 minutes.
        /// </summary>
        public event EventHandler DeviceLinkTimeout;

        /// <summary>
        /// Invoked when an INSTEON device is unlinked from the controller device.
        /// </summary>
        public event InsteonDeviceEventHandler DeviceUnlinked;

        /// <summary>
        /// Cancels linking mode in the controller.
        /// </summary>
        public void CancelLinkMode()
        {
            if (!TryCancelLinkMode())
            {
                throw new IOException();
            }
        }

        /// <summary>
        /// Places the INSTEON controller into linking mode in order to link or unlink a device.
        /// </summary>
        /// <param name="mode">Determines the linking mode for the controller.</param>
        /// <param name="group">Specifies the INSTEON group number to which the device will be linked.</param>
        /// <remarks>
        /// The <see cref="DeviceLinked">DeviceLinked</see> event will be raised when a device has been linked to the controller.
        /// The <see cref="DeviceUnlinked">DeviceUnlinked</see> event will be raised when a device has been unklinked from the controller.
        /// The <see cref="DeviceLinkTimeout">DeviceLinkTimeout</see> event will be raised if a device is not added within the 4 minute timeout period.
        /// </remarks>
        public void EnterLinkMode(InsteonLinkMode mode, byte group)
        {
            if (!TryEnterLinkMode(mode, group))
            {
                throw new IOException();
            }
        }

        /// <summary>
        /// Returns an array of device links in the INSTEON controller.
        /// </summary>
        /// <returns>An array of objects representing each device link.</returns>
        public InsteonDeviceLinkRecord[] GetLinks()
        {
            InsteonDeviceLinkRecord[] links;
            if (!TryGetLinks(out links))
            {
                throw new IOException();
            }
            return links;
        }

        internal void OnMessage(InsteonMessage message)
        {
            if (message.MessageType == InsteonMessageType.DeviceLink)
            {
                var address = new InsteonAddress(message.Properties[PropertyKey.Address]);
                var identity = new InsteonIdentity((byte)message.Properties[PropertyKey.DevCat],
                                                    (byte)message.Properties[PropertyKey.SubCat],
                                                    (byte)message.Properties[PropertyKey.FirmwareVersion]);
                var device = network.Devices.Add(address, identity);
                timer.Stop();
                IsInLinkingMode = false;
                if (linkingMode.HasValue)
                {
                    if (linkingMode != InsteonLinkMode.Delete)
                    {
                        OnDeviceLinked(device);
                    }
                    else
                    {
                        OnDeviceUnlinked(device);
                    }
                }
                else
                {
                    OnDeviceLinked(device);
                }
            }
        }

        private void OnDeviceLinked(InsteonDevice device)
        {
            if (DeviceLinked != null)
            {
                DeviceLinked(this, new InsteonDeviceEventArgs(device));
            }
        }

        private void OnDeviceLinkTimeout()
        {
            if (DeviceLinkTimeout != null)
            {
                DeviceLinkTimeout(this, EventArgs.Empty);
            }
        }

        private void OnDeviceUnlinked(InsteonDevice device)
        {
            if (DeviceUnlinked != null)
            {
                DeviceUnlinked(this, new InsteonDeviceEventArgs(device));
            }
        }


        /// <summary>
        /// Cancels linking mode in the controller.
        /// </summary>
        /// <remarks>
        /// This method does not throw an exception.
        /// </remarks>
        public bool TryCancelLinkMode()
        {
            timer.Stop();
            IsInLinkingMode = false;
            linkingMode = null;
            byte[] message = { (byte)InsteonModemSerialCommand.CancelAllLink };
            logger.DebugFormat("Controller {0} CancelLinkMode", Address.ToString());
            return network.Messenger.TrySend(message) == EchoStatus.ACK;
        }

        /// <summary>
        /// Places the INSTEON controller into linking mode in order to link or unlink a device.
        /// </summary>
        /// <param name="mode">Determines the linking mode as controller, responder, either, or delete.</param>
        /// <param name="group">Specifies the INSTEON group number to which the device will be linked.</param>
        /// <remarks>
        /// The <see cref="DeviceLinked">DeviceLinked</see> event will be raised when a device has been linked to the controller.
        /// The <see cref="DeviceUnlinked">DeviceUnlinked</see> event will be raised when a device has been unklinked from the controller.
        /// The <see cref="DeviceLinkTimeout">DeviceLinkTimeout</see> event will be raised if a device is not added within the 4 minute timeout period.
        /// This method does not throw an exception.
        /// </remarks>
        public bool TryEnterLinkMode(InsteonLinkMode mode, byte group)
        {
            linkingMode = mode;
            byte[] message = { (byte)InsteonModemSerialCommand.StartAllLink, (byte)mode, group };
            logger.DebugFormat("Controller {0} EnterLinkMode(mode:{1}, group:{2:X2})", Address.ToString(), mode.ToString(), group);
            if (network.Messenger.TrySend(message) != EchoStatus.ACK)
            {
                return false;
            }
            timer.Start();
            IsInLinkingMode = true;
            return true;
        }

        /// <summary>
        /// Returns an array of device links in the INSTEON controller.
        /// </summary>
        /// <param name="links">An array of objects representing each device link.</param>
        /// <remarks>
        /// This method does not throw an exception.
        /// </remarks>
        public bool TryGetLinks(out InsteonDeviceLinkRecord[] links)
        {
            links = null;
            var list = new List<InsteonDeviceLinkRecord>();
            Dictionary<PropertyKey, int> properties;
            EchoStatus status;

            logger.DebugFormat("Controller {0} GetLinks", Address.ToString());
            byte[] message1 = { (byte)InsteonModemSerialCommand.GetFirstAllLinkRecord };
            status = network.Messenger.TrySendReceive(message1, false, (byte)InsteonModemSerialCommand.DeviceLinkRecord, null, out properties);

            if (status == EchoStatus.NAK)
            {
                links = new InsteonDeviceLinkRecord[0]; // empty link table
                logger.DebugFormat("Controller {0} GetLinks returned no links, empty link table", Address.ToString());
                return true;
            }
            if (status == EchoStatus.ACK)
            {
                if (properties == null)
                {
                    logger.ErrorFormat("Controller {0} null properties object", Address.ToString());
                    return false;
                }
                list.Add(new InsteonDeviceLinkRecord(properties));
            }
            else
            {
                return false; // echo was not ACK or NAK
            }

            logger.DebugFormat("Controller {0} GetLinks", Address.ToString());
            byte[] message2 = { (byte)InsteonModemSerialCommand.GetNextDeviceLinkRecord };
            status = network.Messenger.TrySendReceive(message2, false, (byte)InsteonModemSerialCommand.DeviceLinkRecord, null, out properties);
            while (status == EchoStatus.ACK)
            {
                if (properties == null)
                {
                    logger.ErrorFormat("Controller {0} null properties object", Address.ToString());
                    return false;
                }
                list.Add(new InsteonDeviceLinkRecord(properties));
                status = network.Messenger.TrySendReceive(message2, false, (byte)InsteonModemSerialCommand.DeviceLinkRecord, null, out properties);
            }

            if (status != EchoStatus.NAK)
            {
                return false; // echo was not ACK or NAK
            }

            links = list.ToArray();
            logger.DebugFormat("Controller {0} GetLinks returned {1} links", Address.ToString(), links.Length);
            return true;
        }

        public bool TryGetLinkIdentity(InsteonDeviceLinkRecord link, out InsteonIdentity? identity)
        {
            //GetProductData(link, out identity) ||
            return  GetLinkIdentity(link, out identity);
        }

        private bool GetLinkIdentity(InsteonDeviceLinkRecord link, out InsteonIdentity? identity)
        {
            Dictionary<PropertyKey, int> properties;

            logger.DebugFormat("Controller {0} GetLinkIdentity", Address.ToString());
            byte[] message = { (byte)InsteonModemSerialCommand.StandardOrExtendedMessage, link.Address[2], link.Address[1], link.Address[0], 
                                 (byte) MessageFlagsStandard.ThreeHopsThreeRemaining, (byte)InsteonDirectCommands.IDRequest, Byte.MinValue };

            var status = network.Messenger.TrySendReceive(message, false, (byte)InsteonModemSerialCommand.StandardMessage, InsteonMessageType.SetButtonPressed, out properties);

            if (status == EchoStatus.NAK)
            {
                logger.ErrorFormat("received NAK trying to get idendity information");
                identity = null;
                return false;
            }
            if (status == EchoStatus.ACK)
            {
                if (properties == null)
                {
                    logger.ErrorFormat("Device Id {0} has null properties object", Address.ToString());
                    identity = null;
                    return false;
                }
                identity = new InsteonIdentity((byte)properties[PropertyKey.DevCat], (byte)properties[PropertyKey.SubCat], (byte)properties[PropertyKey.FirmwareVersion]);
                return true;

            }

            logger.ErrorFormat("received unknown status trying to get idendity information");
            identity = null;
            return false; // echo was not ACK or NAK
        }

        private bool GetProductData(InsteonDeviceLinkRecord link, out InsteonIdentity? identity)
        {
            Dictionary<PropertyKey, int> properties;

            logger.DebugFormat("Controller {0} GetLinkProductData", Address.ToString());
            byte[] message = { (byte)InsteonModemSerialCommand.StandardOrExtendedMessage, link.Address[2], link.Address[1], link.Address[0], 
                                 (byte) MessageFlagsStandard.ThreeHopsThreeRemaining, (byte)InsteonDirectCommands.ProductDataRequest, Byte.MinValue };

            var status = network.Messenger.TrySendReceive(message, false, (byte)InsteonModemSerialCommand.ExtendedMessage, InsteonMessageType.ProductDataResponse, out properties);

            if (status == EchoStatus.NAK)
            {
                logger.ErrorFormat("received NAK trying to get ProductData information");
                identity = null;
                return false;
            }
            if (status == EchoStatus.ACK)
            {
                if (properties == null)
                {
                    logger.ErrorFormat("Device Id {0} has null properties object", Address.ToString());
                    identity = null;
                    return false;
                }
                var pk = new InsteonProductKey((byte)properties[PropertyKey.ProductKeyHigh], (byte)properties[PropertyKey.ProductKeyMid], (byte)properties[PropertyKey.ProductKeyLow]);
                identity = new InsteonIdentity((byte)properties[PropertyKey.DevCat], (byte)properties[PropertyKey.SubCat], (byte)properties[PropertyKey.FirmwareVersion], pk);
                return true;
            }

            logger.ErrorFormat("received unknown status trying to get productdata information");
            identity = null;
            return false; // echo was not ACK or NAK
        }

        private byte[] CreateGroupMessage(InsteonControllerGroupCommands command, byte group, byte value)
        {
            var cmd = (byte)command;
            byte[] message = { (byte)InsteonModemSerialCommand.SendAllLinkCommand, group, cmd, value };
            logger.DebugFormat("Controller {0} GroupCommand(command:{1}, group:{2:X2}, value:{3:X2})", Address.ToString(), command.ToString(), group, value);
            return message;
        }

        /// <summary>
        /// Sends an INSTEON group broadcast command to the controller.
        /// </summary>
        /// <param name="command">Specifies the INSTEON controller group command to be invoked.</param>
        /// <param name="group">Specifies the group number for the command.</param>
        /// <param name="value">A parameter value required by some group commands.</param>
        /// <remarks>
        /// This method does not throw an exception.
        /// This is a non-blocking method that sends an INSTEON message to the target device and returns immediately.
        /// A <see cref="InsteonDevice.DeviceStatusChanged">DeviceStatusChanged</see> event will be invoked for each INSTEON device linked within the specified group that responds to the command.
        /// </remarks>
        public bool TryGroupCommand(InsteonControllerGroupCommands command, byte group, byte value)
        {
            return network.Messenger.TrySend(CreateGroupMessage(command, group, value)) == EchoStatus.ACK;
        }

        /// <summary>
        /// Sends an INSTEON group broadcast command to the controller.
        /// </summary>
        /// <remarks>
        /// This is a non-blocking method that sends an INSTEON message to the target device and returns immediately.
        /// A <see cref="InsteonDevice.DeviceStatusChanged">DeviceStatusChanged</see> event will be invoked for each INSTEON device linked within the specified group that responds to the command.
        /// </remarks>
        /// <param name="command">Specifies the INSTEON controller group command to be invoked.</param>
        /// <param name="group">Specifies the group number for the command.</param>
        /// <param name="value">A parameter value required by some group commands.</param>
        public void GroupCommand(InsteonControllerGroupCommands command, byte group, byte value)
        {
            network.Messenger.Send(CreateGroupMessage(command, group, value));
        }
    }
}