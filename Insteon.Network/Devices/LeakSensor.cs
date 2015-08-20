using System;
using Insteon.Network.Device;
using Insteon.Network.Enum;
using Insteon.Network.Message;
using ServiceStack.Logging;

namespace Insteon.Network.Devices
{
    public class LeakSensor : InsteonDevice
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(LeakSensor));

        public DateTime? LastHeartbeat { get; set; }

        internal LeakSensor(InsteonNetwork network, InsteonAddress address, InsteonIdentity identity)
            : base(network, address, identity)
        {
        }

        internal override void OnMessage(InsteonMessage message)
        {
            var cmd2 = (byte)message.Properties[PropertyKey.Cmd2];

            if (message.MessageType == InsteonMessageType.OnCleanup)
            {
                if (cmd2 == 0x01)
                {
                    logger.InfoFormat("Dry State detected in device {0}", Address.ToString());
                    OnDeviceStatusChanged(InsteonDeviceStatus.DryDetected);
                }
                else if (cmd2 == 0x02)
                {
                    logger.InfoFormat("Wet State detect in device {0}", Address.ToString());
                    OnDeviceStatusChanged(InsteonDeviceStatus.WetDetected);
                }
                else if (cmd2 == 0x04)
                {
                    logger.InfoFormat("Heartbeat from device {0}", Address.ToString());
                    LastHeartbeat = DateTime.Now;
                    OnDeviceStatusChanged(InsteonDeviceStatus.Heartbeat);
                }
            }
            else
            {
                base.OnMessage(message);
            }
        }
    }
}