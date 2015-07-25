using ServiceStack;

namespace Insteon.ServiceModel.Request
{
    [Route("/devices", "GET", Summary = "Get all current Insteon controller links.")]
    public class GetDevices { }
}