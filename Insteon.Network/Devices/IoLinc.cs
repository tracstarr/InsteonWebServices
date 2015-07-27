using Insteon.Network.Device;

namespace Insteon.Network.Devices
{
    public class IoLinc : InsteonDevice
    {
        internal IoLinc(InsteonNetwork network, InsteonAddress address, InsteonIdentity identity)
            : base(network, address, identity)
        {
        }
    }
}
