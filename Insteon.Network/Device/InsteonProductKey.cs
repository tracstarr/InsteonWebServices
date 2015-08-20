using System;
using System.Globalization;

namespace Insteon.Network.Device
{
    public sealed class InsteonProductKey : IComparable<InsteonProductKey>, IEquatable<InsteonProductKey>
    {
        private readonly int productKey;

        public string ProductKey => StringKey();

        public byte KeyHigh => (byte)(productKey >> 16);

        public byte KeyMid => (byte)(productKey >> 8);

        public byte KeyLow => (byte)productKey;

        public byte[] KeyBytes => GetKeyBytes();

        public bool IsValid => Validate();

        public static InsteonProductKey InvalidProductKey { get; }

        static InsteonProductKey()
        {
            InvalidProductKey = new InsteonProductKey(0, 0, 0);
        }

        public InsteonProductKey(byte keyHigh, byte keyMid, byte keyLow)
        {
            productKey = keyHigh << 16 | keyMid << 8 | keyLow;
        }

        public InsteonProductKey(InsteonProductKey key)
        {
            productKey = key.productKey;
        }

        public static implicit operator string(InsteonProductKey key)
        {
            return key.StringKey();
        }

        public static implicit operator byte[](InsteonProductKey key)
        {
            return key.GetKeyBytes();
        }

        public static implicit operator InsteonProductKey(string key)
        {
            return FromString(key);
        }

        public static bool operator ==(InsteonProductKey lhs, InsteonProductKey rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if (lhs == null || rhs == null)
                return false;

            return lhs.productKey == rhs.productKey;
        }

        public static bool operator !=(InsteonProductKey lhs, InsteonProductKey rhs)
        {
            return !(lhs == rhs);
        }

        public string StringKey(char seperator = '.')
        {
            return string.Format("{0:X2}{3}{1:X2}{3}{2:X2}", (object)KeyHigh, (object)KeyMid, (object)KeyLow, (object)seperator);
        }

        public byte[] GetKeyBytes()
        {
            return new[] { KeyHigh, KeyMid, KeyLow };
        }

        public bool Validate()
        {
            return !(this == InvalidProductKey);
        }

        public override string ToString()
        {
            return StringKey();
        }

        public override bool Equals(object obj)
        {
            var insteonProductKey = obj as InsteonProductKey;
            
            if (insteonProductKey == null || !base.Equals(obj))
                return false;
            return this == insteonProductKey;
        }

        public bool Equals(InsteonProductKey p)
        {
            return this == p;
        }

        public override int GetHashCode()
        {
            return productKey.GetHashCode();
        }

        int IComparable<InsteonProductKey>.CompareTo(InsteonProductKey other)
        {
            return productKey.CompareTo(other.productKey);
        }

        bool IEquatable<InsteonProductKey>.Equals(InsteonProductKey other)
        {
            return productKey == other.productKey;
        }

        public static InsteonProductKey FromString(string productKey)
        {
            var num = 0;
            if (productKey.Length == 8)
                num = 1;
            try
            {
                return new InsteonProductKey(byte.Parse(productKey.Substring(0, 2), NumberStyles.HexNumber), byte.Parse(productKey.Substring(2 + num, 2), NumberStyles.HexNumber), byte.Parse(productKey.Substring(4 + num * 2, 2), NumberStyles.HexNumber));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    $"Provided product key '{productKey}' is not in the right format.  INSTEON product keys shouldcontain 3 hexadecimal values with or without seperator.  XX.XX.XX or XXXXXX format.", "productKey", ex);
            }
        }

        public static InsteonProductKey FromBytes(byte[] key)
        {
            return FromBytes(key, 0);
        }

        public static InsteonProductKey FromBytes(byte[] key, int startIndex)
        {
            if (key.Length < 3 + startIndex)
                throw new ArgumentException("At least 3 bytes requred for product key.", nameof(key));
            return new InsteonProductKey(key[startIndex], key[1 + startIndex], key[2 + startIndex]);
        }
    }
}