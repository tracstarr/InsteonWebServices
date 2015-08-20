using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServiceStack.Logging;

namespace Insteon.Network.Serial
{
    // Provides an implementation of the serial communication interface adapting to an INSTEON controller device over a remote socket connection.
    // Rewrite as an async client, see: http://msdn.microsoft.com/en-us/library/bew39x2a(v=vs.110).aspx
    internal class NetDriver : ISerialPort, IDisposable
    {
        private ILog logger = LogManager.GetLogger(typeof(NetDriver));
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
            logger.DebugFormat("NetDriver closing");
            running = false;
            thread.Interrupt();
            thread.Join();
            notify = null;
            logger.DebugFormat("NetDriver closed");
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
                logger.DebugFormat("NetDriver thread no longer running, restarting...");
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
                logger.DebugFormat("NetDriver thread no longer running, restarting...");
                Open();
            }
            //logger.DebugFormat("NetDriver send buffer: {0}", Utilities.ByteArrayToString(data));
            sendBuffer.AddRange(data);
            Thread.Sleep(1); // yield
        }

        public void Wait(int timeout)
        {
            if (!running)
            {
                logger.DebugFormat("NetDriver thread no longer running, restarting...");
                Open();
            }
            wait.WaitOne(timeout);
        }

        private void ThreadProc()
        {
            logger.DebugFormat("NetDriver thread start");
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
                        //logger.DebugFormat("NetDriver send data: {0}", Utilities.ByteArrayToString(data));
                    }

                    if (socket.Poll(100, SelectMode.SelectRead) && socket.Available > 0)
                    {
                        //logger.DebugFormat("NetDriver data available...");
                        while (socket.Available > 0)
                        {
                            var data = new byte[socket.Available];
                            socket.Receive(data, SocketFlags.Partial);
                            receiveBuffer.AddRange(data);
                            //logger.DebugFormat("NetDriver received data: {0}", Utilities.ByteArrayToString(data));
                        }
                        notify?.Invoke();
                        wait.Set();
                    }
                }

                socket.Shutdown(SocketShutdown.Both);
            }
            catch (ThreadInterruptedException)
            {
                //logger.DebugFormat("NetDriver thread connect interrupted");
            }
            catch (SocketException ex)
            {
                logger.ErrorFormat("NetDriver socket error: {0}", ex.Message);
            }

            socket.Close();
            running = false;
            logger.DebugFormat("NetDriver thread exit");
        }
    }
}