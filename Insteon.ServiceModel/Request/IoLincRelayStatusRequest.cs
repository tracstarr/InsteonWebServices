using Insteon.ServiceModel.Response;
using ServiceStack;

namespace Insteon.ServiceModel.Request
{
    [Route("/status/iolinc", "GET", Summary = "Get the status of the IoLinc Relay")]
    public class IoLincStatusRequest : IReturn<IoLinkStatusResponse>
    {
        public string DeviceId { get; set; }
    }
}