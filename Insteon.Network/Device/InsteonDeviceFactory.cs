using System;
using System.Collections.Generic;
using Insteon.Network.Devices;

namespace Insteon.Network.Device
{
    internal static class InsteonDeviceFactory
    {
        public static InsteonDevice GetDevice(this InsteonIdentity insteonIdentity, InsteonNetwork network, InsteonAddress address)
        {
            if (insteonIdentity.IsEmpty)
            {
                return new InsteonDevice(network, address, insteonIdentity);
            }

            if (DeviceTypeLookup.ContainsKey(insteonIdentity.DevCat))
            {
                if (DeviceTypeLookup[insteonIdentity.DevCat].ContainsKey(insteonIdentity.SubCat))
                {
                    return DeviceTypeLookup[insteonIdentity.DevCat][insteonIdentity.SubCat](insteonIdentity, network, address);
                }
            }

            return new InsteonDevice(network, address, insteonIdentity);
        }

        private static readonly Dictionary<byte, Dictionary<byte, Func<InsteonIdentity, InsteonNetwork, InsteonAddress, InsteonDevice>>> DeviceTypeLookup
            = new Dictionary<byte, Dictionary<byte, Func<InsteonIdentity, InsteonNetwork, InsteonAddress, InsteonDevice>>>()
            {
                {
                    //Dimmable Lighting Controls
                    0x01, new Dictionary<byte, Func<InsteonIdentity, InsteonNetwork, InsteonAddress, InsteonDevice>>()
                    {
                        {0x0E, (identity, network, address)=> new DimmableLighting(network, address, identity)},
                        {0x20, (identity, network, address)=> new DimmableLighting(network, address, identity)},
                    }
                },
                {
                    //Switched Lighting Controls
                    0x02, new Dictionary<byte, Func<InsteonIdentity, InsteonNetwork, InsteonAddress, InsteonDevice>>()
                    {
                        {0x0E, (identity, network, address)=> new SwitchedLighting(network, address, identity)},
                    }
                }
            };
    }
}
