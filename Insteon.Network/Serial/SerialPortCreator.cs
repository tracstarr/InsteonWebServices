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
using System.Text;

namespace Insteon.Network.Serial
{
    // Responsible for determing the type of serial interface adapter to invoke based on the specified connection object.
    internal static class SerialPortCreator
    {
        public static ISerialPort Create(InsteonConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException();
            switch (connection.Type)
            {
                case InsteonConnectionType.Net: return new NetDriver(connection.Value);
                case InsteonConnectionType.Serial: return new SerialPortDriver(connection.Value);
            }
            throw new ArgumentException();
        }
    }
}
