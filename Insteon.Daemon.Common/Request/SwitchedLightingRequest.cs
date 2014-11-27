using ServiceStack;

namespace Insteon.Daemon.Common.Request
{
    [Route("/lighting/switched")]
    [Route("/lighting/switched/{DeviceId}/{state}")]
    public class SwitchedLightingRequest : InsteonDeviceRequest
    {
        public bool State { get; set; }
    }
}