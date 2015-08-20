using ServiceStack;

namespace Insteon.ServiceModel.Request
{
    [Route("/device/state", "PUT", Summary = "Send full DTO with deviceId and either on/off.")]
    public class DeviceStateRequest : InsteonDeviceRequest
    {
        public bool State { get; set; }
    }
}