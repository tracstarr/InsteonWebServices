using Insteon.ServiceModel.Response;
using ServiceStack;

namespace Insteon.ServiceModel.Request
{
    [Route("/status/lighting/dimmer", "GET", Summary = "Get the state of the dimmer")]
    public class DimmerStatusRequest : IReturn<DimmerStatusResponse>
    {
        public string DeviceId { get; set; }
    }
}