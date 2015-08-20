using Insteon.Network.Devices;
using Insteon.ServiceModel.Request;
using Insteon.ServiceModel.Response;
using ServiceStack;

namespace Insteon.Daemon.Common.Service
{
    public class IoLincService : DeviceServiceBase, IService
    {
        public IoLincService(InsteonManager manager)
            : base(manager)
        {
        }

        public IoLinkStatusResponse Get(IoLincStatusRequest request)
        {
            var device = FindDevice(request.DeviceId) as IoLinc;
            if (device == null)
            {
                throw HttpError.Unauthorized("Not a valid dimmable lighting device.");
            }

            device.UpdateStatus();

            return new IoLinkStatusResponse()
            {
                Relay = device.RelayStatus.ToString(),
                Sensor = device.SensorStatus.ToString()
            };

        }

    }
}