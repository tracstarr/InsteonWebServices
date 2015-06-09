
using ServiceStack;

namespace Insteon.Daemon.Common.Response
{
    public class InsteonServiceResponse : ResponseStatus
    {
        public string Service { get { return "Insteon Connect"; } }
        public InsteonServiceResponse(string errorCode, string message) : base(errorCode, message) { }
        public InsteonServiceResponse() {}
    }
}
