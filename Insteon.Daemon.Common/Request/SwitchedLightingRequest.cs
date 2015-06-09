using ServiceStack;

namespace Insteon.Daemon.Common.Request
{
    [Route("/lighting/switched", "PUT", Summary = "Send full DTO with deviceId and either on/off.")]
    [Route("/lighting/switched/{DeviceId}/{state}", "PUT", Summary = "Set the device either on or off.")]
    public class SwitchedLightingRequest : InsteonDeviceRequest
    {
        public bool State { get; set; }
    }
}