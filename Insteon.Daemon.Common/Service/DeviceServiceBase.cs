using Insteon.Network.Device;
using ServiceStack;

namespace Insteon.Daemon.Common.Service
{
    public abstract class DeviceServiceBase
    {
        protected readonly InsteonManager Manager;

        protected DeviceServiceBase(InsteonManager manager)
        {
            this.Manager = manager;
        }

        protected string TurnOnOff(string deviceId, bool on)
        {
            var device = FindDevice(deviceId);

            if (on)
                device.TurnOn();
            else
                device.TurnOff();

            return true.ToJson();
        }

        protected InsteonDevice FindDevice(string deviceId)
        {
            InsteonAddress address;
            if (InsteonAddress.TryParse(deviceId, out address))
            {
                if (Manager.Network.Devices.ContainsKey(address))
                {
                    var device = Manager.Network.Devices.Find(address);
                    return device;
                }
                throw HttpError.NotFound("Device does not exist or is not linked.");
            }
            throw HttpError.NotFound("Device does not exist or is not linked.");
        }
    }
}
