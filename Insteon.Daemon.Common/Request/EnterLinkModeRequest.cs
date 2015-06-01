using ServiceStack.ServiceHost;

namespace Insteon.Daemon.Common.Request
{
    [Route("/link/{start}", "PUT", Summary = "Enter/Exit linking mode.")]
    [Route("/link", "PUT", Summary = "Enter/Exit linking mode.")]
    public class EnterLinkModeRequest
    {
        public bool Start { get; set; }
    }
}
