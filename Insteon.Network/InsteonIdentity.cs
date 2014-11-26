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

namespace Insteon.Network
{
    /// <summary>
    /// Represents the INSTEON device type.
    /// </summary>
    public struct InsteonIdentity
    {

        /// <summary>
        /// Initializes a new instance of the InsteonIdentity class.
        /// </summary>
        /// <param name="devCat">The device category representing a product family.</param>
        /// <param name="subCat">The device sub-category representing various types of products within a product family.</param>
        /// <param name="firmwareVersion">The firmware version running within the device.</param>
        public InsteonIdentity(byte devCat, byte subCat, byte firmwareVersion) : this()
        {
            this.DevCat = devCat;
            this.SubCat = subCat;
            this.FirmwareVersion = firmwareVersion;
        }

        /// <summary>
        /// The device category representing a product family.
        /// </summary>
        public byte DevCat { get; private set; }

        /// <summary>
        /// The firmware version running within the device.
        /// </summary>
        public byte FirmwareVersion { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this InsteonIdentity is empty.
        /// </summary>
        public bool IsEmpty { get { return DevCat == 0 && SubCat == 0 && FirmwareVersion == 0; } }

        /// <summary>
        /// The device sub-category representing various types of products within a product family.
        /// </summary>
        public byte SubCat { get; private set; }
    }
}
