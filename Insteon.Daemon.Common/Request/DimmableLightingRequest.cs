using ServiceStack;

namespace Insteon.Daemon.Common.Request
{
    [Route("/lighting/dimmable", "PUT", Summary = "Send DTO to set on level")]
    [Route("/lighting/dimmable/{DeviceId}/{onlevel}", "PUT", Summary = "Set the on level of the given device.")]
    public class DimmableLightingRequest : InsteonDeviceRequest
    {
        public byte OnLevel { get; set; }
    }
}