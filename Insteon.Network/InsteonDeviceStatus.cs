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
    /// Represents the various device states for an INSTEON device.
    /// </summary>
    public enum InsteonDeviceStatus
    {
        /// <summary>
        /// An unknown device state.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Represents the on state for the device.
        /// </summary>
        On = 1,

        /// <summary>
        /// Represents the off state for the device.
        /// </summary>
        Off = 2,

        /// <summary>
        /// Represents the fast on state for the device, which occurs when the paddle button on a supported device is double tapped up.
        /// </summary>
        FastOn = 3,

        /// <summary>
        /// Represents the fast off state for the device, which occurs when the paddle button on a supported device is double tapped down.
        /// </summary>
        FastOff = 4,

        /// <summary>
        /// Represents the brighten state for a dimmer device, which occurs when the paddle button on a supported device is held in the up position.
        /// </summary>
        Brighten = 5,

        /// <summary>
        /// Represents the dim state for a dimmer device, which occurs when the paddle button on a supported device is held in the up position.
        /// </summary>
        Dim = 6
    }
}
