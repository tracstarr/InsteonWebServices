using System.Collections.Generic;
using Insteon.Network.Commands;
using Insteon.Network.Device;
using Insteon.Network.Enum;

namespace Insteon.Network.Message
{
    // This class is responsible for processing raw binary messages into a structured message result that includes the type of message and a property list.
    internal static class InsteonMessageProcessor
    {
        private static bool DeviceLinkCleanupMessage(byte[] data, int offset, out int count, out InsteonMessage message)
        {
            message = null;
            count = 0;
            if (data.Length <= offset + 8)
            {
                return false;
            }

            var messageId = data[offset];
            var properties = new Dictionary<PropertyKey, int>();

            properties[PropertyKey.LinkStatus] = data[offset + 1];
            count = 2;

            message = new InsteonMessage(messageId, InsteonMessageType.DeviceLinkCleanup, properties);
            return true;
        }

        private static bool DeviceLinkMessage(byte[] data, int offset, out int count, out InsteonMessage message)
        {
            message = null;
            count = 0;
            if (data.Length < offset + 9)
            {
                return false;
            }

            var messageId = data[offset];
            var properties = new Dictionary<PropertyKey, int>();

            properties[PropertyKey.LinkType] = data[offset + 1];
            properties[PropertyKey.LinkGroup] = data[offset + 2];
            GetAddressProperty(PropertyKey.Address, data, offset + 3, out count, properties);
            properties[PropertyKey.DevCat] = data[offset + 6];
            properties[PropertyKey.SubCat] = data[offset + 7];
            properties[PropertyKey.FirmwareVersion] = data[offset + 8];
            count = 9;

            message = new InsteonMessage(messageId, InsteonMessageType.DeviceLink, properties);
            return true;
        }

        private static bool DeviceLinkRecordMessage(byte[] data, int offset, out int count, out InsteonMessage message)
        {
            message = null;
            count = 0;
            if (data.Length < offset + 9)
            {
                return false;
            }

            var messageId = data[offset];
            var properties = new Dictionary<PropertyKey, int>();

            properties[PropertyKey.LinkRecordFlags] = data[offset + 1];
            properties[PropertyKey.LinkGroup] = data[offset + 2];
            properties[PropertyKey.LinkAddress] = new InsteonAddress(data[offset + 3], data[offset + 4], data[offset + 5]).Value;
            properties[PropertyKey.LinkData1] = data[offset + 6];
            properties[PropertyKey.LinkData2] = data[offset + 7];
            properties[PropertyKey.LinkData3] = data[offset + 8];
            count = 9;

            message = new InsteonMessage(messageId, InsteonMessageType.DeviceLinkRecord, properties);
            return true;
        }

        private static bool ExtendedMessage(byte[] data, int offset, out int count, out InsteonMessage message)
        {
            message = null;
            count = 0;
            if (data.Length < offset + 23)
            {
                return false;
            }

            StandardMessage(data, offset, out count, out message);
            message.Properties[PropertyKey.Data1] = data[offset + 10];
            message.Properties[PropertyKey.Data2] = data[offset + 11];
            message.Properties[PropertyKey.Data3] = data[offset + 12];
            message.Properties[PropertyKey.Data4] = data[offset + 13];
            message.Properties[PropertyKey.Data5] = data[offset + 14];
            message.Properties[PropertyKey.Data6] = data[offset + 15];
            message.Properties[PropertyKey.Data7] = data[offset + 16];
            message.Properties[PropertyKey.Data8] = data[offset + 17];
            message.Properties[PropertyKey.Data9] = data[offset + 18];
            message.Properties[PropertyKey.Data10] = data[offset + 19];
            message.Properties[PropertyKey.Data11] = data[offset + 20];
            message.Properties[PropertyKey.Data12] = data[offset + 21];
            message.Properties[PropertyKey.Data13] = data[offset + 22];
            message.Properties[PropertyKey.Data14] = data[offset + 23];
            count = 23;
            return true;
        }

        private static bool GetAddressProperty(PropertyKey key, byte[] data, int offset, out int count, Dictionary<PropertyKey, int> properties)
        {
            count = 0;
            if (data.Length <= offset + 2)
            {
                return false;
            }

            properties[key] = data[offset + 0] << 16 | data[offset + 1] << 8 | data[offset + 2];
            count = 2;
            return true;
        }

        private static bool GetInsteonModemInfo(byte[] data, int offset, out int count, out InsteonMessage message)
        {
            message = null;
            count = 0;
            if (data.Length < offset + 7)
            {
                return false;
            }

            var messageId = data[offset];
            var properties = new Dictionary<PropertyKey, int>();

            properties[PropertyKey.Address] = new InsteonAddress(data[offset + 1], data[offset + 2], data[offset + 3]).Value;
            properties[PropertyKey.DevCat] = data[offset + 4];
            properties[PropertyKey.SubCat] = data[offset + 5];
            properties[PropertyKey.FirmwareVersion] = data[offset + 6];
            count = 7;

            message = new InsteonMessage(messageId, InsteonMessageType.GetInsteonModemInfo, properties);
            return true;
        }

        private static bool GetMessageFlagProperty(byte[] data, int offset, out int count, Dictionary<PropertyKey, int> properties)
        {
            count = 0;
            if (data.Length <= offset + 7)
            {
                return false;
            }

            var messageFlags = data[offset + 7];
            properties[PropertyKey.MessageFlagsMaxHops] = messageFlags & (byte)MessageTypeFlags.MaxHops;
            properties[PropertyKey.MessageFlagsRemainingHops] = (messageFlags & (byte)MessageTypeFlags.HopsLeft) >> 2;
            properties[PropertyKey.MessageFlagsExtendedFlag] = (messageFlags & (byte)MessageTypeFlags.Extended) >> 4;
            properties[PropertyKey.MessageFlagsAck] = (messageFlags & (byte)MessageTypeFlags.Ack) >> 5;
            properties[PropertyKey.MessageFlagsAllLink] = (messageFlags & (byte)MessageTypeFlags.AllLink) >> 6;
            properties[PropertyKey.MessageFlagsBroadcast] = (messageFlags & (byte)MessageTypeFlags.Broadcast) >> 7;
            count = 7;
            return true;
        }

        private static InsteonMessageType GetMessageType(byte[] data, int offset, Dictionary<PropertyKey, int> properties)
        {
            var cmd1 = (byte)properties[PropertyKey.Cmd1];
            var ack = properties[PropertyKey.MessageFlagsAck] != 0;
            var broadcast = properties[PropertyKey.MessageFlagsBroadcast] != 0;
            var allLink = properties[PropertyKey.MessageFlagsAllLink] != 0;

            var messageType = InsteonMessageType.Other;
            if (ack)
            {
                messageType = InsteonMessageType.Ack;
            }
            else if (cmd1 == 0x06 && broadcast && allLink)
            {
                messageType = InsteonMessageType.SuccessBroadcast;
                properties[PropertyKey.ResponderCmd1] = data[offset + 4];
                properties[PropertyKey.ResponderCount] = data[offset + 5];
                properties[PropertyKey.ResponderGroup] = data[offset + 6];
                properties[PropertyKey.ResponderErrorCount] = data[offset + 9];
            }
            else if (cmd1 == 0x11 && allLink && broadcast)
            {
                messageType = InsteonMessageType.OnBroadcast;
                properties[PropertyKey.Group] = data[offset + 5];
            }
            else if (cmd1 == 0x11 && allLink && !broadcast)
            {
                messageType = InsteonMessageType.OnCleanup;
                properties[PropertyKey.Group] = data[offset + 9];
            }
            else if (cmd1 == 0x13 && allLink && broadcast)
            {
                messageType = InsteonMessageType.OffBroadcast;
                properties[PropertyKey.Group] = data[offset + 5];
            }
            else if (cmd1 == 0x13 && allLink && !broadcast)
            {
                messageType = InsteonMessageType.OffCleanup;
                properties[PropertyKey.Group] = data[offset + 9];
            }
            else if (cmd1 == 0x12 && allLink && broadcast)
            {
                messageType = InsteonMessageType.FastOnBroadcast;
                properties[PropertyKey.Group] = data[offset + 5];
            }
            else if (cmd1 == 0x12 && allLink && !broadcast)
            {
                messageType = InsteonMessageType.FastOnCleanup;
                properties[PropertyKey.Group] = data[offset + 9];
            }
            else if (cmd1 == 0x14 && allLink && broadcast)
            {
                messageType = InsteonMessageType.FastOffBroadcast;
                properties[PropertyKey.Group] = data[offset + 5];
            }
            else if (cmd1 == 0x14 && allLink && !broadcast)
            {
                messageType = InsteonMessageType.FastOffCleanup;
                properties[PropertyKey.Group] = data[offset + 9];
            }
            else if (cmd1 == 0x17 && allLink && broadcast)
            {
                messageType = InsteonMessageType.IncrementBeginBroadcast;
                properties[PropertyKey.Group] = data[offset + 5];
                properties[PropertyKey.IncrementDirection] = data[offset + 9];
            }
            else if (cmd1 == 0x18 && allLink && broadcast)
            {
                messageType = InsteonMessageType.IncrementEndBroadcast;
                properties[PropertyKey.Group] = data[offset + 5];
            }
            else if ((cmd1 == 0x01 || cmd1 == 0x02) && broadcast)  // 0x01 == responder, 0x02 = controller
            {
                messageType = InsteonMessageType.SetButtonPressed;
                properties[PropertyKey.DevCat] = data[offset + 4];
                properties[PropertyKey.SubCat] = data[offset + 5];
                properties[PropertyKey.FirmwareVersion] = data[offset + 6];
            }

            return messageType;
        }

        public static bool ProcessMessage(byte[] data, int offset, out int count, out InsteonMessage message)
        {
            message = null;
            count = 0;
            if (data.Length <= offset)
            {
                return false;
            }

            switch ((InsteonModemSerialCommandReceived)data[offset])
            {
                case InsteonModemSerialCommandReceived.StandardMessage:
                    return StandardMessage(data, offset, out count, out message);
                case InsteonModemSerialCommandReceived.ExtendedMessage:
                    return ExtendedMessage(data, offset, out count, out message);
                case InsteonModemSerialCommandReceived.DeviceLinkingCompleted:
                    return DeviceLinkMessage(data, offset, out count, out message);
                case InsteonModemSerialCommandReceived.DeviceLinkRecord:
                    return DeviceLinkRecordMessage(data, offset, out count, out message);
                case InsteonModemSerialCommandReceived.DeviceCleanup:
                    return DeviceLinkCleanupMessage(data, offset, out count, out message);
                case InsteonModemSerialCommandReceived.GetImInfo:
                    return GetInsteonModemInfo(data, offset, out count, out message);
            }

            return false;
        }

        private static bool StandardMessage(byte[] data, int offset, out int count, out InsteonMessage message)
        {
            message = null;
            count = 0;
            if (data.Length < offset + 10)
            {
                return false;
            }

            var messageId = data[offset];
            var properties = new Dictionary<PropertyKey, int>();

            GetAddressProperty(PropertyKey.FromAddress, data, offset + 1, out count, properties);
            GetMessageFlagProperty(data, offset, out count, properties);
            if (properties[PropertyKey.MessageFlagsBroadcast] == 0)
            {
                GetAddressProperty(PropertyKey.ToAddress, data, offset + 4, out count, properties);
            }
            properties[PropertyKey.Cmd1] = data[offset + 8];
            properties[PropertyKey.Cmd2] = data[offset + 9];
            count = 10;

            var messageType = GetMessageType(data, offset, properties);
            message = new InsteonMessage(messageId, messageType, properties);
            return true;
        }
    }
}