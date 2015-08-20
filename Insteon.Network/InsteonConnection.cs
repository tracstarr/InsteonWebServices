﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Insteon.Network.Device;
using Insteon.Network.Enum;

namespace Insteon.Network
{
    /// <summary>
    /// Represents a connection to an INSTEON network.
    /// </summary>
    public class InsteonConnection
    {
        /// <summary>
        /// Initializes a new connection instance.
        /// </summary>
        /// <param name="type">Type type of connection.</param>
        /// <param name="value">The connection value.</param>
        public InsteonConnection(InsteonConnectionType type, string value)
            : this(type, value, value, new InsteonAddress()) {}

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
            {
                throw new ArgumentNullException();
            }

            Type = type;
            Value = value.Trim();
            if (!string.IsNullOrEmpty(name) && name.Trim().Length > 0)
            {
                Name = name.Trim();
            }
            else
            {
                Name = value;
            }
            Address = address;
        }

        /// <summary>
        /// The INSTEON address of the controller device.
        /// </summary>
        public InsteonAddress Address { get; internal set; }

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
            {
                return Type == other.Type && string.Equals(Value, other.Value, StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Parses a string into a connection object.
        /// </summary>
        /// <param name="text">The specified connection string.</param>
        /// <returns>Returns the connection object.</returns>
        public static InsteonConnection Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException();
            }

            Regex connectionPattern = new Regex(@"\s*(?<type>[^: ]+)\s*:\s*(?<value>[^, ]+)\s*(,\s*(?<name>[^,]+)\s*)?(,\s*(?<address>[^, ]+))?");
            Match m = connectionPattern.Match(text);
            if (!m.Success)
            {
                throw new FormatException();
            }

            string type = m.Groups["type"].ToString().Trim();
            string value = m.Groups["value"].ToString().Trim();
            string name = m.Groups["name"].ToString().Trim();
            string address = m.Groups["address"].ToString().Trim();

            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(value))
            {
                throw new FormatException();
            }

            type = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(type);

            InsteonConnectionType rtype = (InsteonConnectionType)System.Enum.Parse(typeof(InsteonConnectionType), type);
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
            {
                return $"{Type}: {Value}";
            }
            return Address.IsEmpty ? $"{Type}: {Value}, {name}" : $"{Type}: {Value}, {name}, {Address.ToString()}";
        }
    }
}