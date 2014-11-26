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
    /// Represents the set of commands that can be sent to all INSTEON devices linked to a controller in the specified group.
    /// </summary>
    public enum InsteonControllerGroupCommands
    {
        /// <summary>
        /// Broadcasts a group "on" command to all devices linked to the controller in the specified group.
        /// A parameter value must be specified to determine the group number.
        /// </summary>
        On = 0x11,

        /// <summary>
        /// Broadcasts a group "fast on" command to all devices linked to the controller in the specified group.
        /// A parameter value must be specified to determine the group number.
        /// </summary>
        FastOn = 0x12,

        /// <summary>
        /// Broadcasts a group "off" command to all devices linked to the controller in the specified group.
        /// A parameter value must be specified to determine the group number.
        /// </summary>
        Off = 0x13,

        /// <summary>
        /// Broadcasts a group "fast off" command to all devices linked to the controller in the specified group.
        /// A parameter value must be specified to determine the group number.
        /// </summary>
        FastOff = 0x14,
        
        /// <summary>
        /// Broadcasts a group "brighten" command to all devices linked to the controller in the specified group.
        /// A parameter value must be specified to determine the group number.
        /// </summary>
        Brighten = 0x15,

        /// <summary>
        /// Broadcasts a group "dim" command to all devices linked to the controller in the specified group.
        /// A parameter value must be specified to determine the group number.
        /// </summary>
        Dim = 0x16,

        /// <summary>
        /// Broadcasts a group "start dimming" command to all devices linked to the controller in the specified group.
        /// All dimmers linked to the specified group will start dimming.
        /// Dimming will continue until a stop dimming command is received or a limit is reached. 
        /// A parameter value must be specified to determine the group number.
        /// A second parameter value must be specified to determine the direction (0=darken, 1=brighten).
        /// </summary>
        StartDimming = 0x17,

        /// <summary>
        /// Broadcasts a group "stop dimming" command to all devices linked to the controller in the specified group.
        /// All dimmers linked to the specified group will stop dimming.
        /// Dimming will continue until a stop dimming command is received or a limit is reached. 
        /// A parameter value must be specified to determine the group number.
        /// </summary>
        StopDimming = 0x18
    }
}
