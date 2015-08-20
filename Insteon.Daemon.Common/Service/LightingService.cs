using System;
using Insteon.Network.Devices;
using Insteon.ServiceModel.Request;
using Insteon.ServiceModel.Response;
using ServiceStack;

namespace Insteon.Daemon.Common.Service
{
    public class LightingService : DeviceServiceBase, IService
    {
        public LightingService(InsteonManager manager)
            : base(manager)
        {

        }

        public string Put(SwitchedLightingRequest request)
        {
            return TurnOnOff(request.DeviceId, request.State);
        }

        public string Put(DimmableLightingRequest request)
        {
            var device = FindDevice(request.DeviceId) as DimmableLighting;

            if (device == null)
            {
                throw HttpError.Unauthorized("Not a valid dimmable lighting device.");
            }

            if (request.State && !request.Fast)
                device.RampOn(request.Level);
            else if (request.State && request.Fast)
                device.TurnOn(request.Level);
            else if (!request.State && !request.Fast)
                device.RampOff();
            else
                device.TurnOff();

            return true.ToJson();
        }

        public DimmerStatusResponse Get(DimmerStatusRequest request)
        {
            var device = FindDevice(request.DeviceId);
            if (device == null)
            {
                throw HttpError.Unauthorized("Not a valid dimmable lighting device.");
            }

            byte value;
            if (device.TryGetStatus(out value))
            {
                return new DimmerStatusResponse() { DeviceId = request.DeviceId, Level = value };
            }
            throw new Exception("No response from device " + request.DeviceId);

        }

    }
}
