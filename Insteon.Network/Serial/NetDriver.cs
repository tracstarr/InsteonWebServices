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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Insteon.Network.Serial
{
    // Provides an implementation of the serial communication interface adapting to an INSTEON controller device over a remote socket connection.
    // Rewrite as an async client, see: http://msdn.microsoft.com/en-us/library/bew39x2a(v=vs.110).aspx
    internal class NetDriver : ISerialPort, IDisposable
    {
        private const int port = 9761;
        private readonly string host = string.Empty;
        private readonly IPAddress address = IPAddress.None;
        private DataAvailable notify = null;
        private Thread thread = null;
        private bool running = false;
        private readonly List<byte> sendBuffer = new List<byte>();        
        private readonly List<byte> receiveBuffer = new List<byte>();
        private readonly AutoResetEvent wait = new AutoResetEvent(false);

        public NetDriver(string host)
        {
            if (!IPAddress.TryParse(host, out address))
                this.host = host;
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

        public void Dispose()
        {
            Close();            
        }

        public void Open()
        {
            thread = new Thread(this.ThreadProc);
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

            byte[] data = receiveBuffer.ToArray();
            receiveBuffer.Clear();
            return data;
        }

        public void SetNotify(DataAvailable notify)
        {
            this.notify = notify;
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
                    socket.Connect(host, port);
                else
                    socket.Connect(address, port);

                while (running)
                {
                    if (sendBuffer.Count > 0)
                    {
                        byte[] data = sendBuffer.ToArray();
                        sendBuffer.Clear();
                        socket.Send(data, SocketFlags.None);
                        //Log.WriteLine("NetDriver send data: {0}", Utilities.ByteArrayToString(data));
                    }

                    if (socket.Poll(100, SelectMode.SelectRead) && socket.Available > 0)
                    {
                        //Log.WriteLine("NetDriver data available...");
                        while (socket.Available > 0)
                        {
                            byte[] data = new byte[socket.Available];
                            socket.Receive(data, SocketFlags.Partial);
                            receiveBuffer.AddRange(data);
                            //Log.WriteLine("NetDriver received data: {0}", Utilities.ByteArrayToString(data));
                        }
                        if (notify != null)
                            notify();
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
    }
}
