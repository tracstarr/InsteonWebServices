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
    // Identfies the type of INSTEON message.
    internal enum InsteonMessageType
    {
        Other = 0,
        Ack,
        DeviceLink,
        DeviceLinkCleanup,
        DeviceLinkRecord,
        FastOffBroadcast,
        FastOffCleanup,
        FastOnBroadcast,
        FastOnCleanup,
        GetIMInfo,
        IncrementBeginBroadcast,
        IncrementEndBroadcast,
        OffBroadcast,
        OffCleanup,
        OnBroadcast,
        OnCleanup,
        SetButtonPressed,
        SuccessBroadcast
    }
}
