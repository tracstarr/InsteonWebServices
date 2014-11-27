using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Insteon.Network.Helpers;

namespace Insteon.Network.Serial
{
    // Provides an implementation of the serial communication interface adapting to an INSTEON controller device over a remote socket connection.
    // Rewrite as an async client, see: http://msdn.microsoft.com/en-us/library/bew39x2a(v=vs.110).aspx
    internal class NetDriver : ISerialPort, IDisposable
    {
        private const int port = 9761;
        private readonly IPAddress address = IPAddress.None;
        private readonly string host = string.Empty;
        private readonly List<byte> receiveBuffer = new List<byte>();
        private readonly List<byte> sendBuffer = new List<byte>();
        private readonly AutoResetEvent wait = new AutoResetEvent(false);
        private DataAvailable notify;
        private bool running;
        private Thread thread;

        public NetDriver(string host)
        {
            if (!IPAddress.TryParse(host, out address))
            {
                this.host = host;
            }
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            Log.WriteLine("NetDriver closing");
            running = false;
            thread.Interrupt();
            thread.Join();
            notify = null;
            Log.WriteLine("NetDriver closed");
        }

        public void Open()
        {
            thread = new Thread(ThreadProc);
            thread.Start();
            Thread.Sleep(100); // yield
        }

        public byte[] ReadAll()
        {
            if (!running)
            {
                Log.WriteLine("NetDriver thread no longer running, restarting...");
                Open();
            }

            var data = receiveBuffer.ToArray();
            receiveBuffer.Clear();
            return data;
        }

        public void SetNotify(DataAvailable notify)
        {
            this.notify = notify;
        }

        public void Write(byte[] data)
        {
            if (!running)
            {
                Log.WriteLine("NetDriver thread no longer running, restarting...");
                Open();
            }
            //Log.WriteLine("NetDriver send buffer: {0}", Utilities.ByteArrayToString(data));
            sendBuffer.AddRange(data);
            Thread.Sleep(1); // yield
        }

        public void Wait(int timeout)
        {
            if (!running)
            {
                Log.WriteLine("NetDriver thread no longer running, restarting...");
                Open();
            }
            wait.WaitOne(timeout);
        }

        private void ThreadProc()
        {
            Log.WriteLine("NetDriver thread start");
            running = true;
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;

            try
            {
                if (!string.IsNullOrEmpty(host))
                {
                    socket.Connect(host, port);
                }
                else
                {
                    socket.Connect(address, port);
                }

                while (running)
                {
                    if (sendBuffer.Count > 0)
                    {
                        var data = sendBuffer.ToArray();
                        sendBuffer.Clear();
                        socket.Send(data, SocketFlags.None);
                        //Log.WriteLine("NetDriver send data: {0}", Utilities.ByteArrayToString(data));
                    }

                    if (socket.Poll(100, SelectMode.SelectRead) && socket.Available > 0)
                    {
                        //Log.WriteLine("NetDriver data available...");
                        while (socket.Available > 0)
                        {
                            var data = new byte[socket.Available];
                            socket.Receive(data, SocketFlags.Partial);
                            receiveBuffer.AddRange(data);
                            //Log.WriteLine("NetDriver received data: {0}", Utilities.ByteArrayToString(data));
                        }
                        if (notify != null)
                        {
                            notify();
                        }
                        wait.Set();
                    }
                }

                socket.Shutdown(SocketShutdown.Both);
            }
            catch (ThreadInterruptedException)
            {
                //Log.WriteLine("NetDriver thread connect interrupted");
            }
            catch (SocketException ex)
            {
                Log.WriteLine("NetDriver socket error: {0}", ex.Message);
            }

            socket.Close();
            running = false;
            Log.WriteLine("NetDriver thread exit");
        }
    }
}