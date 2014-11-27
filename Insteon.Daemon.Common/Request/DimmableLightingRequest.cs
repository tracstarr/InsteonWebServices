using ServiceStack;

namespace Insteon.Daemon.Common.Request
{
    [Route("/lighting/dimmable")]
    [Route("/lighting/dimmable/{DeviceId}/{onlevel}")]
    public class DimmableLightingRequest : InsteonDeviceRequest
    {
        public byte OnLevel { get; set; }
    }
}