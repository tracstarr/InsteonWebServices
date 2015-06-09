using ServiceStack;

namespace Insteon.Daemon.Common.Request
{
    [Route("/status", "GET", Summary = "Get the current status of your Insteon Controller.")]
    public class GetStatus { }
}