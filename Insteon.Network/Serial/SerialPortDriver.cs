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
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Insteon.Network.Serial
{
    // Provides an implementation of the serial communication interface adapting to an INSTEON controller device over a local serial connection.
    internal class SerialPortDriver : ISerialPort, IDisposable
    {
        private SerialPort port = null;
        private readonly AutoResetEvent wait = new AutoResetEvent(false);
        private DataAvailable notify = null;

        public SerialPortDriver(string name)
        {
            port = new SerialPort(name, 19200, Parity.None, 8, StopBits.One);
        }

        public void Close()
        {
            if (port != null && port.IsOpen)
                port.Close();
            notify = null;
        }

        public void Dispose()
        {
            Close();
            port.Dispose();
        }

        public void Open()
        {
            if (port != null)
                port.Close();
            port.Open();
            port.DataReceived += port_DataReceived;
        }

        public byte[] ReadAll()
        {
            if (!port.IsOpen)
                port.Open();
            int count = port.BytesToRead;
            byte[] data = new byte[count];
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
                port.Open();
            if (data == null)
                throw new ArgumentNullException("data");
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
                notify();
            wait.Set();
        }
    }
}
