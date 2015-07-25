using ServiceStack;

namespace Insteon.ServiceModel.Request
{
    [Route("/status", "GET", Summary = "Get the current status of your Insteon Controller.")]
    public class GetStatus { }
}