using ServiceStack;

namespace Insteon.ServiceModel.Request
{
    [Route("/lighting/switched", "PUT", Summary = "Send full DTO with deviceId and either on/off.")]
    public class SwitchedLightingRequest : InsteonDeviceRequest
    {
        public bool State { get; set; }
    }
}