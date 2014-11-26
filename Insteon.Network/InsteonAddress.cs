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
using System.Globalization;
using System.Text;

namespace Insteon.Network
{
    /// <summary>
    /// Represents an INSTEON device address. Example: 19.9E.4E.
    /// </summary>
    public struct InsteonAddress
    {
        private readonly int value;

        /// <summary>
        /// Initializes a new instance of the InsteonAddress class with an integer.
        /// </summary>
        /// <param name="address">An integer representation of the INSTEON address. Example: 0x199E4E.</param>
        public InsteonAddress(int address)
        {
            value = address;
        }

        /// <summary>
        /// Initializes a new instance of the InsteonAddress class with a set of three byte values.
        /// </summary>
        /// <param name="a2">The high order byte part of the INSTEON address.</param>
        /// <param name="a1">The middle order byte part of the INSTEON address.</param>
        /// <param name="a0">The low order byte part of the INSTEON address.</param>
        public InsteonAddress(byte a2, byte a1, byte a0)
        {
            value = a0 | a1 << 8 | a2 << 16;
        }

        /// <summary>
        /// Returns the specified part of the INSTEON address.
        /// </summary>
        /// <param name="index">Specifies the byte part to be returned. Valid indexes are 0, 1, and 2.</param>
        /// <returns>A byte part of the INSTEON address.</returns>
        public byte this[int index]
        { 
            get 
            {
                switch (index)
                {
                    case 0: return (byte)((value & 0x0000FF) >> 0);
                    case 1: return (byte)((value & 0x00FF00) >> 8);
                    case 2: return (byte)((value & 0xFF0000) >> 16);
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Formats the INSTEON address as a string.
        /// </summary>
        /// <param name="address">The specified INSTEON address, example: 0x199E4E.</param>
        /// <returns>A string representation of the resulting INSTEON address.</returns>
        public static string Format(int address)
        {
            return new InsteonAddress(address).ToString();
        }

        /// <summary>
        /// Gets a value indicating whether this InsteonAddress is empty.
        /// </summary>
        public bool IsEmpty { get { return value == 0; } }

        /// <summary>
        /// Converts the string representation of an INSTEON address to its numeric equivalent.
        /// </summary>
        /// <param name="value">A string specifying the INSTEON address, example: "19.9E.4A".</param>
        /// <returns>Returns an object representing the resulting INSTEON address.</returns>
        public static InsteonAddress Parse(string value)
        {
            InsteonAddress address;
            if (!TryParse(value, out address))
                throw new FormatException();
            return address;
        }

        /// <summary>
        /// Converts the string representation of an INSTEON address to its numeric equivalent.
        /// </summary>
        /// <param name="value">A string specifying the INSTEON address, example: "19.9E.4E".</param>
        /// <param name="address">An object representing the resulting INSTEON address.</param>
        /// <returns>Returns true if the string was successfully parsed.</returns>
        /// <remarks>
        /// This method does not throw an exception.
        /// </remarks>
        public static bool TryParse(string value, out InsteonAddress address)
        {
            address = new InsteonAddress();
            if (string.IsNullOrEmpty(value))
                return false;
            value = value.Trim();
            if (value.Length != 8)
                return false;
            if (value[2] != '.' || value[5] != '.')
                return false;
            byte a0 = byte.Parse(value.Substring(6, 2), NumberStyles.HexNumber);
            byte a1 = byte.Parse(value.Substring(3, 2), NumberStyles.HexNumber);
            byte a2 = byte.Parse(value.Substring(0, 2), NumberStyles.HexNumber);
            address = new InsteonAddress(a2, a1, a0);
            return true;
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation.
        /// </summary>
        public override string ToString()
        {            
            int a0 = (value & 0x0000FF) >> 0;
            int a1 = (value & 0x00FF00) >> 8;
            int a2 = (value & 0xFF0000) >> 16;
            return string.Format("{0:X2}.{1:X2}.{2:X2}", a2, a1, a0);
        }

        /// <summary>
        /// Returns the integer representation of the INSTEON address.
        /// </summary>
        public int Value { get { return value; } }
    }

}
