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
    /// <summary>
    /// Represents a link to another device within an INSTEON device's link table.
    /// </summary>
    public struct InsteonDeviceLinkRecord
    {
        internal InsteonDeviceLinkRecord(Dictionary<PropertyKey, int> properties)
        : this(
            properties[PropertyKey.LinkAddress],
            (byte)properties[PropertyKey.LinkGroup],
            (byte)properties[PropertyKey.LinkData1],
            (byte)properties[PropertyKey.LinkData2],
            (byte)properties[PropertyKey.LinkData3],
            (byte)properties[PropertyKey.LinkRecordFlags]
            )
        {
        }
        private InsteonDeviceLinkRecord(int address, byte group, byte data1, byte data2, byte data3, byte flags) : this()
        {
            this.Address = new InsteonAddress(address);
            this.Group = group;
            this.Data1 = data1;
            this.Data2 = data2;
            this.Data3 = data3;
            this.LinkRecordFlags = flags;
        }

        /// <summary>
        /// The INSTEON address of the device link.
        /// </summary>
        public InsteonAddress Address { get; private set; }
        
        /// <summary>
        /// Device specific link information for the device link.
        /// For a responder link, this value typically represents the on-level.
        /// For a controller link, this value specifies the number of times to retry the command.
        /// </summary>
        public byte Data1 { get; private set; }

        /// <summary>
        /// Device specific link information for the device link.
        /// For a responder link, this value typically represents the ramp rate.
        /// </summary>
        public byte Data2 { get; private set; }

        /// <summary>
        /// Device specific link information for the device link.
        /// For a responder link in a KeypadLinc device, this value typically represents the keypad button number.
        /// </summary>
        public byte Data3 { get; private set; }

        /// <summary>
        /// The group number of the device link.
        /// </summary>
        public byte Group { get; private set; }
        
        /// <summary>
        /// Indicates if the device link is in use, and whether it is a controller or a responder link.
        /// </summary>
        public byte LinkRecordFlags { get; private set; }
        
        /// <summary>
        /// Determines whether the device link is a controller link or a responder link.
        /// </summary>
        public InsteonDeviceLinkRecordType RecordType
        {
            get
            {
                if ((LinkRecordFlags & 0x80) == 0)
                    return InsteonDeviceLinkRecordType.Empty;
                else if ((LinkRecordFlags & 0x40) != 0)
                    return InsteonDeviceLinkRecordType.Controller;
                else
                    return InsteonDeviceLinkRecordType.Responder;
            }
        }
    }

    /// <summary>
    /// Represents the type of link record.
    /// </summary>
    public enum InsteonDeviceLinkRecordType
    {
        /// <summary>
        /// Indicates the link record is empty, or not in use.
        /// </summary>
        Empty = 0,
        
        /// <summary>
        /// Indicates the link record is a controller link, allowing messages to be sent to the linked device.
        /// </summary>
        Controller,
        
        /// <summary>
        /// Indicates the link record is a responder link, allowing messages to be received from the linked device.
        /// </summary>
        Responder
    }
}
