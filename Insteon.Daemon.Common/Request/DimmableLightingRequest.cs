using ServiceStack;

namespace Insteon.Daemon.Common.Request
{
    [Route("/lighting/dimmable", "PUT", Summary = "Send DTO to set on level")]
    public class DimmableLightingRequest : InsteonDeviceRequest
    {
        public byte Level { get; set; }
        public bool State { get; set; }
        public bool Fast { get; set; }
    }
}