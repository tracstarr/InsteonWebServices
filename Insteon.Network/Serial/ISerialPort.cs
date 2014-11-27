namespace Insteon.Network.Serial
{
    // Provides an abstract serial communication interface to an INSTEON controller device.
    internal interface ISerialPort
    {
        void Close();
        void Open();
        byte[] ReadAll();
        void SetNotify(DataAvailable notify);
        void Write(byte[] data);
        void Wait(int timeout);
    }

    internal delegate void DataAvailable();
}