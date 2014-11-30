using System.Collections.Generic;
using ServiceStack.ServiceInterface.ServiceModel;

namespace Insteon.Daemon.Common.Response
{
    public class GetDevicesResponse : IHasResponseStatus
    {		
        public IList<string> Result { get; set; }		
        public ResponseStatus ResponseStatus { get; set; }
    }
}