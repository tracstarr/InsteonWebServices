using System;
using Insteon.Network.Device;
using Insteon.Network.Devices;
using Insteon.ServiceModel.Request;
using Insteon.ServiceModel.Response;
using ServiceStack;

namespace Insteon.Daemon.Common.Service
{
    public class LightingService : IService
    {
        private readonly InsteonManager manager;

        public LightingService(InsteonManager manager)
        {
            this.manager = manager;
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

        public DimmerStatusResponse Get(DimmerStatusRequest request)
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

                    byte value;
                    if (device.TryGetOnLevel(out value))
                    {
                        return new DimmerStatusResponse() { DeviceId = request.DeviceId, Level = value };
                    }
                    throw new Exception("No response from device " + request.DeviceId);
                }
                else
                {
                    throw HttpError.NotFound("Device does not exist or is not linked.");
                }
            }

            throw HttpError.NotFound("Device does not exist or is not linked.");
        }

    }
}
