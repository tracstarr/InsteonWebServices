using System;
using System.IO.Ports;
using System.Threading;

namespace Insteon.Network.Serial
{
    // Provides an implementation of the serial communication interface adapting to an INSTEON controller device over a local serial connection.
    internal class SerialPortDriver : ISerialPort, IDisposable
    {
        private readonly SerialPort port;
        private readonly AutoResetEvent wait = new AutoResetEvent(false);
        private DataAvailable notify;

        public SerialPortDriver(string name)
        {
            port = new SerialPort(name, 19200, Parity.None, 8, StopBits.One);
        }

        public void Dispose()
        {
            Close();
            port.Dispose();
        }

        public void Close()
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
            }
            notify = null;
        }

        public void Open()
        {
            if (port != null)
            {
                port.Close();
            }
            port.Open();
            port.DataReceived += port_DataReceived;
        }

        public byte[] ReadAll()
        {
            if (!port.IsOpen)
            {
                port.Open();
            }
            int count = port.BytesToRead;
            var data = new byte[count];
            port.Read(data, 0, count);
            return data;
        }

        public void SetNotify(DataAvailable notify)
        {
            this.notify = notify;
        }

        public void Write(byte[] data)
        {
            if (!port.IsOpen)
            {
                port.Open();
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            port.Write(data, 0, data.Length);
        }

        public void Wait(int timeout)
        {
            wait.Reset();
            wait.WaitOne(timeout);
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (notify != null)
            {
                notify();
            }
            wait.Set();
        }
    }
}