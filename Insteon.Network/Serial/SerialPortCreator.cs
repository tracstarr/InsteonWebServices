using System;
using Insteon.Network.Enum;

namespace Insteon.Network.Serial
{
    // Responsible for determing the type of serial interface adapter to invoke based on the specified connection object.
    internal static class SerialPortCreator
    {
        public static ISerialPort Create(InsteonConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException();
            }
            switch (connection.Type)
            {
                case InsteonConnectionType.Net:
                    return new NetDriver(connection.Value);
                case InsteonConnectionType.Serial:
                    return new SerialPortDriver(connection.Value);
            }
            throw new ArgumentException();
        }
    }
}