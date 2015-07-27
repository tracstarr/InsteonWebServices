using Insteon.Network.Device;

namespace Insteon.Network.Devices
{
    public class MotionSensor : InsteonDevice
    {
        internal MotionSensor(InsteonNetwork network, InsteonAddress address, InsteonIdentity identity)
            : base(network, address, identity)
        {
        }
    }
}