using System.Collections.Generic;
using Insteon.Network.Device;

namespace Insteon.Network.Helpers
{
    public static class InsteonCategoryExtensions
    {
        public static string GetDeviceCategoryName(this InsteonIdentity insteonIdentity)
        {
            return DeviceCategoryLookup[insteonIdentity.DevCat];
        }

        public static string GetSubCategoryName(this InsteonIdentity insteonIdentity)
        {
            return DeviceSubcategoryLookup[insteonIdentity.DevCat][insteonIdentity.SubCat];
        }

        private static readonly Dictionary<byte, string> DeviceCategoryLookup
           = new Dictionary<byte, string>()
            {
                { 0x00, "Generalized Controllers" },
                { 0x01, "Dimmable Lighting Control" },
                { 0x02, "Switched Lighting Control" },
                { 0x03, "Network Bridges" },
                { 0x04, "Irrigation Control" },
                { 0x05, "Climate Control" },
                { 0x06, "Pool and Spa Control" },
                { 0x07, "Sensors and Actuators" },
                { 0x08, "Home Entertainment" },
                { 0x09, "Energy Management" },
                { 0x0A, "Built-In Appliance Control" },
                { 0x0B, "Plumbing" },
                { 0x0C, "Communication" },
                { 0x0D, "Computer Control" },
                { 0x0E, "Window Coverings" },
                { 0x0F, "Access Control" },
                { 0x10, "Security, Health, Safety" },
                { 0x11, "Surveillance" },
                { 0x12, "Automotive" },
                { 0x13, "Pet Care" },
                { 0x14, "Toys" },
                { 0x15, "Timekeeping" },
                { 0x16, "Holiday" },
                { 0xFF, "Unassigned" }
            };

        private static readonly Dictionary<byte, Dictionary<byte, string>> DeviceSubcategoryLookup
            = new Dictionary<byte, Dictionary<byte, string>>()
            {
                { 0x00, new Dictionary<byte,string>() 
                    { 
                        {0x00, "Unknown"},
                        {0x04, "ControlLinc [2430]"},
                        {0x05, "RemoteLinc [2440]"},
                        {0x06, "Icon Tabletop Controller [2830]"},
                        {0x08, "EZBridge/EZServer"},
                        {0x09, "SignalLinc RF Signal Enhancer [2442]"},
                        {0x0A, "Balboa Instrument's Poolux LCD Controller"},
                        {0x0B, "Access Point [2443]"},
                        {0x0C, "IES Color Touchscreen"},
                        {0x0D, "SmartLabs KeyFOB"},
                        {0x11, "Mini Remote - Switch [2444A3]"},
                        {0x12, "Mini Remote - 8 Scene [2444A2WH8]"},
                    } },
                { 0x01, new Dictionary<byte,string>() 
                    { 
                        {0x00, "LampLinc V2 [2456D3]"},
                        {0x01, "SwitchLinc V2 Dimmer 600W [2476D]"},
                        {0x02, "In-LineLinc Dimmer [2475D]"},
                        {0x03, "Icon Switch Dimmer [2876D]"},
                        {0x04, "SwitchLinc V2 Dimmer 1000W [2476DH]"},
                        {0x05, "KeypadLinc Dimmer Countdown Timer [2484DWH8]"},
                        {0x06, "LampLinc 2-Pin [2456D2]"},
                        {0x07, "Icon LampLinc V2 2-Pin [2856D2]"},
                        {0x08, "SwitchLinc Dimmer Count-down Timer [2484DWH8]"},
                        {0x09, "KeypadLinc Dimmer [2486D]"},
                        {0x0A, "Icon In-Wall Controller [2886D]"},
                        {0x0B, "Access Point LampLinc [2458D3]"},
                        {0x0C, "KeypadLinc Dimmer - 8-Button defaulted mode [2486DWH8]"},
                        {0x0D, "SocketLinc [2454D]"},
                        {0x0E, "LampLinc Dimmer, Dual-Band [2457D3]"},
                        {0x13, "ICON SwitchLinc Dimmer for Lixar/Bell Canada [2676D-B]"},
                        {0x17, "ToggleLinc Dimmer [2466D]"},
                        {0x18, "Icon SL Dimmer Inline Companion [2474D]"},
                        {0x19, "SwitchLinc 800W"},
                        {0x1A, "In-LineLinc Dimmer with Sense [2475D2]"},
                        {0x1B, "KeypadLinc 6-button Dimmer [2486DWH6]"},
                        {0x1C, "KeypadLinc 8-button Dimmer [2486DWH8]"},
                        {0x1D, "SwitchLinc Dimmer 1200W [2476D]"},
                        {0x20, "SwitchLinc Dimmer Dual Band [2477D]"}
                    } },
                { 0x02, new Dictionary<byte,string>() 
                    { 
                        {0x05, "KeypadLinc Relay - 8-Button defaulted mode [2486SWH8]"},
                        {0x06, "Outdoor ApplianceLinc [2456S3E]"},
                        {0x07, "TimerLinc [2456ST3]"},
                        {0x08, "OutletLinc [2473S]"},
                        {0x09, "ApplianceLinc [2456S3]"},
                        {0x0A, "SwitchLinc Relay [2476S]"},
                        {0x0B, "Icon On Off Switch [2876S]"},
                        {0x0C, "Icon Appliance Adapter [2856S3]"},
                        {0x0D, "ToggleLinc Relay [2466S]"},
                        {0x0E, "SwitchLinc Relay Countdown Timer [2476ST]"},
                        {0x0F, "KeypadLinc On/Off Switch [2486SWH6]"},
                        {0x10, "In-LineLinc Relay [2475D]"},
                        {0x11, "EZSwitch30 (240V, 30A load controller)"},
                        {0x12, "Icon SL Relay Inline Companion"},
                        {0x13, "ICON SwitchLinc Relay for Lixar/Bell Canada [2676R-B]"},
                        {0x14, "In-LineLinc Relay with Sense [2475S2]"},
                        {0x16, "SwitchLinc Relay with Sense [2476S2]"},
                        {0x2A, "SwitchLinc Relay [2477S]"},
                        {0x38, "On/Off Outdoor Module [2634-222]"},
                    } },
                { 0x03, new Dictionary<byte,string>() 
                    { 
                        {0x01, "PowerLinc Serial [2414S]"},
                        {0x02, "PowerLinc USB [2414U]"},
                        {0x03, "Icon PowerLinc Serial [2814S]"},
                        {0x04, "Icon PowerLinx USB [2814U]"},
                        {0x05, "SmartLabs PowerLinc Modem Serial [2412S]"},
                        {0x06, "SmartLabs IR to Insteon Interface [2411R]"},
                        {0x07, "SmartLabs IRLinc - IR Transmitter Interface [2411T]"},
                        {0x08, "SmartLabs Bi-Directional IR -Insteon Interface"},
                        {0x09, "SmartLabs RF Developer's Board [2600RF]"},
                        {0x0A, "SmartLabs PowerLinc Modem Ethernet [2412E]"},
                        {0x0B, "SmartLabs PowerLinc Modem USB [2412U]"},
                        {0x0C, "SmartLabs PLM Alert Serial"},
                        {0x0D, "SimpleHomeNet EZX 10RF"},
                        {0x0E, "X10 TW-523/PSC05 Translator"},
                        {0x0F, "EZX10IR (X10 IR receiver, Insteon controller and IR distribution hub)"},
                        {0x10, "SmartLinc 2412N INSTEON Central Controller"},
                        {0x11, "PowerLinc - Serial (Dual Band) [2413S]"},
                        {0x12, "RF Modem Card"},
                        {0x13, "PowerLinc USB - HouseLinc 2 enabled [2412UH]"},
                        {0x14, "PowerLinc Serial - HouseLinc 2 enabled [2412SH]"},
                        {0x15, "PowerLinc - USB (Dual Band) [2413U]"}
                    } },
                { 0x04, new Dictionary<byte,string>() 
                    { 
                        {0x00, "Compacta EZRain Sprinkler Controller"}
                    } },
                { 0x05, new Dictionary<byte,string>() 
                    { 
                        {0x00, "Broan SMSC080 Exhaust Fan"},
                        {0x01, "Compacta EZTherm"},
                        {0x02, "Broan SMSC110 Exhaust Fan"},
                        {0x03, "INSTEON Thermostat Adapter [2441V]"},
                        {0x04, "Compacta EZThermx Thermostat"},
                        {0x05, "Broan, Venmar, BEST Rangehoods"},
                        {0x06, "Broan SmartSense Make-up Damper"}
                    } },
                { 0x06, new Dictionary<byte,string>() 
                    { 
                        {0x00, "Compacta EZPool"},
                        {0x01, "Low-end pool controller (Temp. Eng. Project name)"},
                        {0x02, "Mid-Range pool controller (Temp. Eng. Project name)"},
                        {0x03, "Next Generation pool controller (Temp. Eng. Project name)"}
                    } },
                { 0x07, new Dictionary<byte,string>() 
                    { 
                        {0x00, "IOLinc [2450]"},
                        {0x01, "Compacta EZSns1W Sensor Interface Module"},
                        {0x02, "Compacta EZIO8T I/O Module"},
                        {0x03, "Compacta EZIO2X4 #5010D INSTEON / X10 Input/Output Module"},
                        {0x04, "Compacta EZIO8SA I/O Module"},
                        {0x05, "Compacta EZSnsRF #5010E RF Receiver Interface Module for Dakota Alerts Products"},
                        {0x06, "Compacta EZISnsRf Sensor Interface Module"},
                        {0x07, "EZIO6I (6 inputs)"},
                        {0x08, "EZIO4O (4 relay outputs)"}
                    } },
                { 0x09, new Dictionary<byte,string>() 
                    { 
                        {0x00, "Compacta EZEnergy"},
                        {0x01, "OnSitePro Leak Detector"},
                        {0x02, "OnsitePro Control Valve"},
                        {0x03, "Energy Inc. TED 5000 Single Phase Measuring Transmitting Unit (MTU)"},
                        {0x04, "Energy Inc. TED 5000 Gateway - USB"},
                        {0x05, "Energy Inc. TED 5000 Gateway - Ethernet"},
                        {0x06, "Energy Inc. TED 3000 Three Phase Measuring Transmitting Unit (MTU)"},
                    } },
                { 0x0E, new Dictionary<byte,string>() 
                    { 
                        {0x00, "Somfy Drape Controller RF Bridge"}
                    } },
                { 0x0F, new Dictionary<byte,string>() 
                    { 
                        {0x00, "Weiland Doors' Central Drive and Controller"},
                        {0x01, "Weiland Doors' Secondary Central Drive"},
                        {0x02, "Weiland Doors' Assist Drive"},
                        {0x03, "Weiland Doors' Elevation Drive"}
                    } },
                { 0x10, new Dictionary<byte,string>() 
                    { 
                        {0x00, "First Alert ONELink RF to Insteon Bridge"},
                        {0x01, "Motion Sensor [2420M]"},
                        {0x02, "TriggerLinc - INSTEON Open / Close Sensor [2421]"},
                        {0x08, "Leak Sensor [2852-222]"}
                    } }
            };

    }
}
