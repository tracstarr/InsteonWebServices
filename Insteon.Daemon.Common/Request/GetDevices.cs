using ServiceStack.ServiceHost;

namespace Insteon.Daemon.Common.Request
{
    [Route("/devices", "GET", Summary = "Get all current Insteon controller links.")]
    public class GetDevices { }
}