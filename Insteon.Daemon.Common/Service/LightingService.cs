using Insteon.Daemon.Common.Request;
using Insteon.Network.Device;
using Insteon.Network.Devices;
using ServiceStack;

namespace Insteon.Daemon.Common.Service
{
    public class LightingService : IService
    {
        private readonly InsteonManager manager;
        private readonly SmartThingsSettings settings;

        public LightingService(InsteonManager manager, SmartThingsSettings settings)
        {
            this.manager = manager;
            this.settings = settings;
        }


        public ResponseStatus Put(SwitchedLightingRequest request)
        {
            InsteonAddress address;
            if (InsteonAddress.TryParse(request.DeviceId, out address))
            {
                if (manager.Network.Devices.ContainsKey(address))
                {
                    var device = manager.Network.Devices.Find(address) as SwitchedLighting;
                    if (device == null)
                    {
                        return new ResponseStatus("403", "Not a valid switched lighting device.");
                    }
                    if (request.State)
                        device.TurnOn();
                    else if (!request.State)
                        device.TurnOff();
                }
                else
                {
                    return new ResponseStatus("403", "Device does not exist or is not linked.");
                }
            }
            else
            {
                return new ResponseStatus("404");
            }


            return new ResponseStatus();

        }

    }
}
