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
using System.Text;

namespace Insteon.Network
{
    // This class is responsible for processing raw binary messages into a structured message result that includes the type of message and a property list.
    internal static class InsteonMessageProcessor
    {
        private static bool DeviceLinkCleanupMessage(byte[] data, int offset, out int count, out InsteonMessage message)
        {
            message = null;
            count = 0;
            if (data.Length <= offset + 8)
                return false;

            byte messageId = data[offset];
            InsteonMessageType messageType = InsteonMessageType.DeviceLinkCleanup;
            Dictionary<PropertyKey, int> properties = new Dictionary<PropertyKey, int>();

            properties[PropertyKey.LinkStatus] = data[offset + 1];
            count = 2;

            message = new InsteonMessage(messageId, messageType, properties);
            return true;
        }

        private static bool DeviceLinkMessage(byte[] data, int offset, out int count, out InsteonMessage message)
        {
            message = null;
            count = 0;
            if (data.Length < offset + 9)
                return false;

            byte messageId = data[offset];
            InsteonMessageType messageType = InsteonMessageType.DeviceLink;
            Dictionary<PropertyKey, int> properties = new Dictionary<PropertyKey, int>();

            properties[PropertyKey.LinkType] = data[offset + 1];
            properties[PropertyKey.LinkGroup] = data[offset + 2];
            GetAddressProperty(PropertyKey.Address, data, offset + 3, out count, properties);
            properties[PropertyKey.DevCat] = data[offset + 6];
            properties[PropertyKey.SubCat] = data[offset + 7];
            properties[PropertyKey.FirmwareVersion] = data[offset + 8];
            count = 9;

            message = new InsteonMessage(messageId, messageType, properties);
            return true;
        }

        private static bool DeviceLinkRecordMessage(byte[] data, int offset, out int count, out InsteonMessage message)
        {
            message = null;
            count = 0;
            if (data.Length < offset + 9)
                return false;

            byte messageId = data[offset];
            InsteonMessageType messageType = InsteonMessageType.DeviceLinkRecord;
            Dictionary<PropertyKey, int> properties = new Dictionary<PropertyKey, int>();
            
            properties[PropertyKey.LinkRecordFlags] = data[offset + 1];
            properties[PropertyKey.LinkGroup] = data[offset + 2];
            properties[PropertyKey.LinkAddress] = new InsteonAddress(data[offset + 3], data[offset + 4], data[offset + 5]).Value;
            properties[PropertyKey.LinkData1] = data[offset + 6];
            properties[PropertyKey.LinkData2] = data[offset + 7];
            properties[PropertyKey.LinkData3] = data[offset + 8];
            count = 9;

            message = new InsteonMessage(messageId, messageType, properties);
            return true;
        }

        private static bool ExtendedMessage(byte[] data, int offset, out int count, out InsteonMessage message)
        {
            message = null;
            count = 0;
            if (data.Length < offset + 23)
                return false;

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
                return false;

            properties[key] = data[offset + 0] << 16 | data[offset + 1] << 8 | data[offset + 2];
            count = 2;
            return true;
        }

        private static bool GetIMInfo(byte[] data, int offset, out int count, out InsteonMessage message)
        {
            message = null;
            count = 0;
            if (data.Length < offset + 7)
                return false;

            byte messageId = data[offset];
            InsteonMessageType messageType = InsteonMessageType.GetIMInfo;
            Dictionary<PropertyKey, int> properties = new Dictionary<PropertyKey, int>();

            properties[PropertyKey.Address] = new InsteonAddress(data[offset + 1], data[offset + 2], data[offset + 3]).Value;
            properties[PropertyKey.DevCat] = data[offset + 4];
            properties[PropertyKey.SubCat] = data[offset + 5];
            properties[PropertyKey.FirmwareVersion] = data[offset + 6];
            count = 7;

            message = new InsteonMessage(messageId, messageType, properties);
            return true;
        }

        private static bool GetMessageFlagProperty(byte[] data, int offset, out int count, Dictionary<PropertyKey, int> properties)
        {
            count = 0;
            if (data.Length <= offset + 7)
                return false;

            byte messageFlags = data[offset + 7];
            properties[PropertyKey.MessageFlagsMaxHops] = messageFlags & 0x03;
            properties[PropertyKey.MessageFlagsRemainingHops] = (messageFlags & 0x0C) >> 2;
            properties[PropertyKey.MessageFlagsExtendedFlag] = (messageFlags & 0x10) >> 4;
            properties[PropertyKey.MessageFlagsAck] = (messageFlags & 0x20) >> 5;
            properties[PropertyKey.MessageFlagsAllLink] = (messageFlags & 0x40) >> 6;
            properties[PropertyKey.MessageFlagsBroadcast] = (messageFlags & 0x80) >> 7;
            count = 7;
            return true;
        }

        private static InsteonMessageType GetMessageType(byte[] data, int offset, Dictionary<PropertyKey, int> properties)
        {
            byte cmd1 = (byte)properties[PropertyKey.Cmd1];
            bool ack = properties[PropertyKey.MessageFlagsAck] != 0;
            bool broadcast = properties[PropertyKey.MessageFlagsBroadcast] != 0;
            bool allLink = properties[PropertyKey.MessageFlagsAllLink] != 0;

            InsteonMessageType messageType = InsteonMessageType.Other;
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
            else if (cmd1 == 0x01 || cmd1 == 0x02)
            {
                messageType = InsteonMessageType.SetButtonPressed;
                properties[PropertyKey.DevCat] = data[offset + 3];
                properties[PropertyKey.SubCat] = data[offset + 4];
                properties[PropertyKey.FirmwareVersion] = data[offset + 5];
            }

            return messageType;
        }

        public static bool ProcessMessage(byte[] data, int offset, out int count, out InsteonMessage message)
        {
            message = null;
            count = 0;
            if (data.Length <= offset)
                return false;

            switch (data[offset])
            {
                case 0x50: return InsteonMessageProcessor.StandardMessage(data, offset, out count, out message);
                case 0x51: return InsteonMessageProcessor.ExtendedMessage(data, offset, out count, out message);
                case 0x53: return InsteonMessageProcessor.DeviceLinkMessage(data, offset, out count, out message);
                case 0x57: return InsteonMessageProcessor.DeviceLinkRecordMessage(data, offset, out count, out message);
                case 0x58: return InsteonMessageProcessor.DeviceLinkCleanupMessage(data, offset, out count, out message);
                case 0x60: return InsteonMessageProcessor.GetIMInfo(data, offset, out count, out message);
            }

            return false;
        }

        private static bool StandardMessage(byte[] data, int offset, out int count, out InsteonMessage message)
        {
            message = null;
            count = 0;
            if (data.Length < offset + 10)
                return false;

            byte messageId = data[offset];
            Dictionary<PropertyKey, int> properties = new Dictionary<PropertyKey, int>();

            GetAddressProperty(PropertyKey.FromAddress, data, offset + 1, out count, properties);
            GetMessageFlagProperty(data, offset, out count, properties);
            if (properties[PropertyKey.MessageFlagsBroadcast] == 0)
                GetAddressProperty(PropertyKey.ToAddress, data, offset + 4, out count, properties);
            properties[PropertyKey.Cmd1] = data[offset + 8];
            properties[PropertyKey.Cmd2] = data[offset + 9];
            count = 10;

            InsteonMessageType messageType = GetMessageType(data, offset, properties);
            message = new InsteonMessage(messageId, messageType, properties);
            return true;
        }
    }
}
