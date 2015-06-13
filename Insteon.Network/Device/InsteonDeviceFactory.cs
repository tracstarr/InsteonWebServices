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
                    // Generalized Controllers
                    0x00, new Dictionary<byte, Func<InsteonIdentity, InsteonNetwork, InsteonAddress, InsteonDevice>>()
                    {
                        {0x11, (identity, network, address)=> new InsteonDevice(network, address, identity)},    // Mini Remote - Switch [2444A3]
                        {0x12, (identity, network, address)=> new InsteonDevice(network, address, identity)},    // Mini Remote - 8 Scene [2444A2WH8]
                    }
                },
                {
                    // Dimmable Lighting Controls
                    0x01, new Dictionary<byte, Func<InsteonIdentity, InsteonNetwork, InsteonAddress, InsteonDevice>>()
                    {
                        {0x0E, (identity, network, address)=> new DimmableLighting(network, address, identity)},    // LampLinc Dimmer [2457D2]
                        {0x20, (identity, network, address)=> new DimmableLighting(network, address, identity)},    // SwitchLinc Dimmer [2477D]
                    }
                },
                {
                    // Switched Lighting Controls
                    0x02, new Dictionary<byte, Func<InsteonIdentity, InsteonNetwork, InsteonAddress, InsteonDevice>>()
                    {
                        {0x2A, (identity, network, address)=> new SwitchedLighting(network, address, identity)},        // Switchlink relay [2477S]
                        {0x08, (identity, network, address)=> new SwitchedLighting(network, address, identity)},        // Outlink relay [2473SWH]
                        {0x38, (identity, network, address)=> new SwitchedLighting(network, address, identity)},        // On/Off Outdoor Module [2634-222]
                    }
                },
                {
                    // Network Bridges
                    0x03, new Dictionary<byte, Func<InsteonIdentity, InsteonNetwork, InsteonAddress, InsteonDevice>>()
                    {
                        {0x15, (identity, network, address)=> new InsteonDevice(network, address, identity)},        // PowerLinc USB Modem [2413U]
                    }
                },
                {
                    // Sensors and Actuators
                    0x07, new Dictionary<byte, Func<InsteonIdentity, InsteonNetwork, InsteonAddress, InsteonDevice>>()
                    {
                        {0x00, (identity, network, address)=> new InsteonDevice(network, address, identity)},        // I/O Linc [2450]
                    }
                },
                {
                    // Security, Health, Safety
                    0x10, new Dictionary<byte, Func<InsteonIdentity, InsteonNetwork, InsteonAddress, InsteonDevice>>()
                    {
                        {0x01, (identity, network, address)=> new InsteonDevice(network, address, identity)},        // Motion Sensor [2842-222]
                    }
                },
                
            };
    }
}
