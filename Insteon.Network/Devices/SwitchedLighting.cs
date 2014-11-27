using Insteon.Network.Commands;
using Insteon.Network.Device;
using Insteon.Network.Enum;

namespace Insteon.Network.Devices
{
    public class SwitchedLighting : InsteonDevice
    {
        internal SwitchedLighting(InsteonNetwork network, InsteonAddress address, InsteonIdentity identity) : base(network, address, identity) { }

        /// <summary>
        /// Commands the lighting device to turn on immediately
        /// </summary>
        /// <returns>True if the device responds with an ACK</returns>
        public bool TurnOn()
        {
            return TryCommand(InsteonDirectCommands.FastOn, (byte) DeviceLevelEnum.FullOn); 
        }

        /// <summary>
        /// Commands the lighting device to turn off immediately
        /// </summary>
        /// <returns>True if the device responds with an ACK</returns>
        public bool TurnOff()
        {
            return TryCommand(InsteonDirectCommands.FastOff, (byte) DeviceLevelEnum.Off);
        }
    }


}
