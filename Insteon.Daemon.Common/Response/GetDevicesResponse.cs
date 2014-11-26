using System.Collections;
using System.Collections.Generic;
using Insteon.Network;
using ServiceStack;

namespace Insteon.Daemon.Common.Response
{
    public class GetDevicesResponse : IHasResponseStatus
    {		
        public IList<string> Result { get; set; }		
        public ResponseStatus ResponseStatus { get; set; }
    }
}