using ServiceStack;

namespace Insteon.ServiceModel.Request
{
    [Route("/links/refresh", "GET", Summary = "Refresh all device links from PLM")]
    public class RefreshDeviceLinksRequest
    {
    }
}