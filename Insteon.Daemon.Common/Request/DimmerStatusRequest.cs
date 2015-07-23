using Insteon.Daemon.Common.Response;
using ServiceStack;

namespace Insteon.Daemon.Common.Request
{
    [Route("/status/lighting/dimmer", "GET", Summary = "Get the state of the dimmer")]
    public class DimmerStatusRequest : IReturn<DimmerStatusResponse>
    {
        public string DeviceId { get; set; }
    }
}