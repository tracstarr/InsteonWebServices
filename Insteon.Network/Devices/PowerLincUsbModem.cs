using Insteon.Network.Device;

namespace Insteon.Network.Devices
{
    public class PowerLincUsbModem : InsteonDevice
    {
        internal PowerLincUsbModem(InsteonNetwork network, InsteonAddress address, InsteonIdentity identity)
            : base(network, address, identity)
        {
        }
    }
}