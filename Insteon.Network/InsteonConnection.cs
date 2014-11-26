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
using System.Text.RegularExpressions;

namespace Insteon.Network
{
    /// <summary>
    /// Represents a connection to an INSTEON network.
    /// </summary>
    public class InsteonConnection
    {
        /// <summary>
        /// The INSTEON address of the controller device.
        /// </summary>
        public InsteonAddress Address { get; private set; }

        /// <summary>
        /// Determines whether this connection is the same as the specified connection.
        /// </summary>
        /// <remarks>
        /// Equality is determined by comparing the Type and Value properties.
        /// The Name and Address properties are not used in the comparison as they are intended for informational display purposes only.
        /// </remarks>
        /// <param name="other">The specified other connection object.</param>
        /// <returns>Returns true if this connection is the same as the other connection.</returns>
        public bool Equals(InsteonConnection other)
        {
            if (other != null)
                return Type == other.Type && string.Equals(Value, other.Value, StringComparison.InvariantCultureIgnoreCase);
            else
                return false;
        }

        /// <summary>
        /// The display name for the connection.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The type of connection (e.g., network or serial).
        /// </summary>
        public InsteonConnectionType Type { get; private set; }
        
        /// <summary>
        /// Value that specifies the network address or serial port (e.g., "192.168.1.1" or "COM3").
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Initializes a new connection instance.
        /// </summary>
        /// <param name="type">Type type of connection.</param>
        /// <param name="value">The connection value.</param>
        public InsteonConnection(InsteonConnectionType type, string value)
        : this(type, value, value, new InsteonAddress())
        {
        }

        /// <summary>
        /// Initializes a new connection instance.
        /// </summary>
        /// <param name="type">Type type of connection.</param>
        /// <param name="value">The connection value.</param>
        /// <param name="name">The display name for the connection.</param>
        /// <param name="address">The INSTEON address of the controller device.</param>
        public InsteonConnection(InsteonConnectionType type, string value, string name, InsteonAddress address)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException();

            this.Type = type;
            this.Value = value.Trim();
            if (!string.IsNullOrEmpty(name) && name.Trim().Length > 0)
                this.Name = name.Trim();
            else
                this.Name = value;
            this.Address = address;
        }

        /// <summary>
        /// Parses a string into a connection object.
        /// </summary>
        /// <param name="text">The specified connection string.</param>
        /// <returns>Returns the connection object.</returns>
        public static InsteonConnection Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException();

            Regex connectionPattern = new Regex(@"\s*(?<type>[^: ]+)\s*:\s*(?<value>[^, ]+)\s*(,\s*(?<name>[^,]+)\s*)?(,\s*(?<address>[^, ]+))?");
            Match m = connectionPattern.Match(text);
            if (!m.Success)
                throw new FormatException();

            string type = m.Groups["type"].ToString().Trim();
            string value = m.Groups["value"].ToString().Trim();
            string name = m.Groups["name"].ToString().Trim();
            string address = m.Groups["address"].ToString().Trim();

            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(value))
                throw new FormatException();

            type = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(type);
            
            InsteonConnectionType rtype = (InsteonConnectionType)Enum.Parse(typeof(InsteonConnectionType), type);
            InsteonAddress raddress = !string.IsNullOrEmpty(address) ? InsteonAddress.Parse(address) : new InsteonAddress();

            return new InsteonConnection(rtype, value, name, raddress);
        }

        /// <summary>
        /// Parses a string into a connection string object.
        /// </summary>
        /// <param name="text">The specified connection string.</param>
        /// <param name="connection">The returned connection object.</param>
        /// <returns>Returns true if the string could be parsed.</returns>
        public static bool TryParse(string text, out InsteonConnection connection)
        {
            try
            {
                connection = Parse(text);
                return true;
            }
            catch (ArgumentException)
            {
                connection = null;
                return false;
            }
            catch (FormatException)
            {
                connection = null;
                return false;
            }
        }

        /// <summary>
        /// Converts this instance to its equivalent string representation.
        /// </summary>
        public override string ToString()
        {
            string name = (Name != Value) ? Name : string.Empty;
            if (string.IsNullOrEmpty(name) && Address.IsEmpty)
                return string.Format("{0}: {1}", Type, Value);
            else if (Address.IsEmpty)
                return string.Format("{0}: {1}, {2}", Type, Value, name);
            else
                return string.Format("{0}: {1}, {2}, {3}", Type, Value, name, Address.ToString());
        }
    }
}
