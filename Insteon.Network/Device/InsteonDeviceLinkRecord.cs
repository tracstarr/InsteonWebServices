using System.Collections.Generic;
using Insteon.Network.Enum;

namespace Insteon.Network.Device
{
    /// <summary>
    /// Represents a link to another device within an INSTEON device's link table.
    /// </summary>
    public struct InsteonDeviceLinkRecord
    {
        internal InsteonDeviceLinkRecord(Dictionary<PropertyKey, int> properties)
            : this(
                properties[PropertyKey.LinkAddress],
                (byte) properties[PropertyKey.LinkGroup],
                (byte) properties[PropertyKey.LinkData1],
                (byte) properties[PropertyKey.LinkData2],
                (byte) properties[PropertyKey.LinkData3],
                (byte) properties[PropertyKey.LinkRecordFlags]
                ) {}

        private InsteonDeviceLinkRecord(int address, byte group, byte data1, byte data2, byte data3, byte flags) : this()
        {
            Address = new InsteonAddress(address);
            Group = group;
            Data1 = data1;
            Data2 = data2;
            Data3 = data3;
            LinkRecordFlags = flags;
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
                // Bit7 (1) Record in use, (0) record is available. This will always be 1 (docs)
                if ((LinkRecordFlags & 0x80) == 0)
                {
                    return InsteonDeviceLinkRecordType.Empty;
                }

                //TODO: if I compare to HouseLinc, they show opposite lists that actually make sense. I've switched R vs C here

                // Bit 6 (0) Responder (slave)
                // Bit 6 (1) Controller
                // SWITCHED ^^^ which is what documentation shows?

                if ((LinkRecordFlags & 0x40) == 0)
                {
                    return InsteonDeviceLinkRecordType.Controller;
                }

                
                return InsteonDeviceLinkRecordType.Responder;
            }
        }
    }
}