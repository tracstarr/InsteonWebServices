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

namespace Insteon.Network
{
    /// <summary>
    /// Represents the set of commands that can be sent to an INSTEON device.
    /// </summary>
    public enum InsteonDeviceCommands
    {        
        /// <summary>
        /// Commands a device to enter linking mode.
        /// </summary>
        EnterLinkingMode = 0x09,
        
        /// <summary>
        /// Commands a device to enter unlinking mode.
        /// </summary>
        EnterUnlinkingMode = 0x0A,
        
        /// <summary>
        /// Queries a device for its INSTEON identity (DevCat, SubCat, FirmwareVersion).
        /// </summary>
        IDRequest = 0x10,
        
        /// <summary>
        /// Commands a device to turn on. If the target device is a dimmer, a second parameter determines the brightness level from 0 (least brightness) to 255 (full brightness).
        /// </summary>
        On = 0x11,
        
        /// <summary>
        /// Commands a device to turn on immediately, causing dimmer devices to bypass their slow ramp up behavior.
        /// </summary>
        FastOn = 0x12,
        
        /// <summary>
        /// Commands a device to turn off.
        /// </summary>
        Off = 0x13,
        
        /// <summary>
        /// Commands a device to turn off immediately, causing dimmer devices to bypass their slow ramp down behavior.
        /// </summary>
        FastOff = 0x14,
        
        /// <summary>
        /// Commands a dimmer device to brighten incrementally by one step, where each step is a small percentage of the full on level for the device.
        /// </summary>
        Brighten = 0x15,
        
        /// <summary>
        /// Commands a dimmer device to dim incrementally by one step, where each step is a small percentage of the full on level for the device.
        /// </summary>
        Dim = 0x16,
        
        /// <summary>
        /// Commands a dimmer device to slowly begin dimming. Dimming will continue until a stop dimming command is received or a limit is reached.
        /// A parameter value must be specified to determine the direction (0=dim, 1=brighten).
        /// </summary>
        StartDimming = 0x17,
        
        /// <summary>
        /// Commands a dimmer device to stop the dimming process initiated by a previous start dimming command.
        /// </summary>
        StopDimming = 0x18,

        /// <summary>
        /// Gets the on-level for the device.
        /// </summary>
        StatusRequest = 0x19
    }
}
