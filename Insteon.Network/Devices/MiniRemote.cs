using Insteon.Network.Device;

namespace Insteon.Network.Devices
{
    public class MiniRemote : InsteonDevice
    {
        public int NumberOfButtons { get; private set; }

        internal MiniRemote(InsteonNetwork network, InsteonAddress address, InsteonIdentity identity, int buttonCount)
            : base(network, address, identity)
        {
            NumberOfButtons = buttonCount;
        }
    }
}
