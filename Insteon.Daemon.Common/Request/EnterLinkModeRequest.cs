using ServiceStack;

namespace Insteon.Daemon.Common.Request
{
    [Route("/link/{start}")]
    public class EnterLinkModeRequest
    {
        public bool Start { get; set; }
    }
}
