using ServiceStack;

namespace Insteon.Daemon.Common.Request
{
    [Route("/switch")]
    [Route("/switch/{DeviceId}/{state}")]
    public class SwitchRequest
    {
        public string DeviceId { get; set; }
        public bool State { get; set; }
    }
}