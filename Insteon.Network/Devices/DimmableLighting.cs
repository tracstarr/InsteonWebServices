using System;
using Insteon.Network.Commands;
using Insteon.Network.Device;

namespace Insteon.Network.Devices
{
    public class DimmableLighting : SwitchedLighting
    {
        internal DimmableLighting(InsteonNetwork network, InsteonAddress address, InsteonIdentity identity)
            : base(network, address, identity)
        {

        }

        /// <summary>
        /// Commands the lighting device to turn on immediately,
        /// ignoring ramp rate, to the set level.
        /// </summary>
        /// <param name="onLevel">Light level (0-255)</param>
        /// <returns>True if the device responds with an ACK</returns>
        public bool TurnOn(byte onLevel)
        {
            return TryCommand(InsteonDirectCommands.FastOn, onLevel); 
        }

        /// <summary>
        /// Commands the lighting device to turn on using the saved
        /// ramp rate, to the set level.
        /// </summary>
        /// <param name="onLevel">Light level (0-255)</param>
        /// <returns>True if the device responds with an ACK</returns>
        public bool RampOn(byte onLevel)
        {
            return TryCommand(InsteonDirectCommands.On, onLevel);
        }

        /// <summary>
        /// Commands the lighting device to turn off using the saved
        /// ramp rate.
        /// </summary>
        /// <returns>True if the device responds with an ACK</returns>
        public bool RampOff()
        {
            return TryCommand(InsteonDirectCommands.Off, Byte.MinValue);
        }

        /// <summary>
        /// Commands the lighting device to brighten one level.  There
        /// are 32 brightness levels.
        /// </summary>
        /// <returns>True if the device responds with an ACK</returns>
        public bool BrightenOneStep()
        {
           // return base.Plm.Network
             //   .SendStandardCommandToAddress(base.DeviceId, 0x15, 0x00);
            return false;
        }

        /// <summary>
        /// Commands the lighting device to dim one level.  There
        /// are 32 brightness levels.
        /// </summary>
        /// <returns>True if the device responds with an ACK</returns>
        public bool DimOneStep()
        {
            //return base.Plm.Network
            //    .SendStandardCommandToAddress(base.DeviceId, 0x16, 0x00);
            return false;
        }

        /// <summary>
        /// Commands the lighting device to begin ramping up the on-level.
        /// Use StopBrighteningOrDimming to cancel.
        /// </summary>
        /// <returns>True if the device responds with an ACK</returns>
        public bool BeginBrightening()
        {
            //return base.Plm.Network
            //    .SendStandardCommandToAddress(base.DeviceId, 0x17, 0x01);
            return false;
        }

        /// <summary>
        /// Commands the lighting device to begin ramping down the on-level.
        /// Use StopBrighteningOrDimming to cancel.
        /// </summary>
        /// <returns>True if the device responds with an ACK</returns>
        public bool BeginDimming()
        {
            //return base.Plm.Network
                //.SendStandardCommandToAddress(base.DeviceId, 0x17, 0x00);
            return false;
        }

        /// <summary>
        /// Commands the lighting device to stop the ramping of the 
        /// on-level that was started with BeginBrightening or BeginDimming
        /// </summary>
        /// <returns>True if the device responds with an ACK</returns>
        public bool StopBrighteningOrDimming()
        {
            //return base.Plm.Network
            //    .SendStandardCommandToAddress(base.DeviceId, 0x18, 0x00);
            return false;
        }
    }
}
