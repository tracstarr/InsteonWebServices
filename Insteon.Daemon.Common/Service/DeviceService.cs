using Insteon.ServiceModel.Request;
using ServiceStack;

namespace Insteon.Daemon.Common.Service
{
    public class DeviceService : DeviceServiceBase, IService
    {
        public DeviceService(InsteonManager manager)
            : base(manager)
        {

        }

        public string Put(DeviceStateRequest request)
        {
            return base.TurnOnOff(request.DeviceId, request.State);
        }

    }
}