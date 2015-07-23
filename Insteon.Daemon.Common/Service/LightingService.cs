using System;
using System.Web.Configuration;
using Insteon.Daemon.Common.Request;
using Insteon.Daemon.Common.Settings;
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
        
        public string Put(SwitchedLightingRequest request)
        {
            InsteonAddress address;
            if (InsteonAddress.TryParse(request.DeviceId, out address))
            {
                if (manager.Network.Devices.ContainsKey(address))
                {
                    var device = manager.Network.Devices.Find(address) as SwitchedLighting;
                    if (device == null)
                    {
                        throw HttpError.Unauthorized("Not a valid switched lighting device.");
                    }
                    if (request.State)
                        device.TurnOn();
                    else if (!request.State)
                        device.TurnOff();
                }
                else
                {
                    throw HttpError.NotFound("Device does not exist or is not linked.");
                }
            }
            else
            {
                throw HttpError.NotFound("Device does not exist or is not linked.");
            }

            return true.ToJson();

        }

        public string Put(DimmableLightingRequest request)
        {
            InsteonAddress address;
            if (InsteonAddress.TryParse(request.DeviceId, out address))
            {
                if (manager.Network.Devices.ContainsKey(address))
                {
                    var device = manager.Network.Devices.Find(address) as DimmableLighting;
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
                }
                else
                {
                    throw HttpError.NotFound("Device does not exist or is not linked.");
                }
            }
            else
            {
                throw HttpError.NotFound("Device does not exist or is not linked.");
            }

            return true.ToJson();
        }

    }
}
