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
    /// Determines the linking mode for the EnterLinkMode method of the INSTEON controller device.
    /// </summary>
    public enum InsteonLinkMode
    {
        /// <summary>
        /// Specifies to create a responder link within the controller and a corresponding controller link in the linked device.
        /// </summary>
        Responder = 0x00,

        /// <summary>
        /// Specifies to create a controller link within the controller and a corresponding responder link in the linked device.
        /// </summary>
        Controller = 0x01,

        /// <summary>
        /// Determines the type of link based on which device was the first to enter linking mode, the first device to enter linking mode will be the controller.
        /// </summary>
        Either = 0x03,

        /// <summary>
        /// Specifies to delete the links from the controller and the linked device.
        /// </summary>
        Delete = 0xFF
    }
}
