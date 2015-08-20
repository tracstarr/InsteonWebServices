using Insteon.Network.Device;
using Insteon.Network.Enum;
using Insteon.Network.Message;
using ServiceStack.Logging;

namespace Insteon.Network.Devices
{
    public class MotionSensor : InsteonDevice
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(MotionSensor));

        public bool LowBattery { get; set; }

        internal MotionSensor(InsteonNetwork network, InsteonAddress address, InsteonIdentity identity)
            : base(network, address, identity)
        {
        }

        internal override void OnMessage(InsteonMessage message)
        {
            var cmd2 = (byte)message.Properties[PropertyKey.Cmd2];
            
            if (cmd2 == 0x03 && message.MessageType == InsteonMessageType.OnCleanup)
            {
                logger.WarnFormat("Low battery in device {0}", Address.ToString());
                LowBattery = true;
                OnDeviceStatusChanged(InsteonDeviceStatus.LowBattery);
            }
            else if (cmd2 == 0x02 && message.MessageType == InsteonMessageType.OffCleanup)
            {
                logger.WarnFormat("Light detect in device {0}", Address.ToString());
                OnDeviceStatusChanged(InsteonDeviceStatus.LightDetected);
            }
            else
            {
                base.OnMessage(message);
            }
        }

        /*internal override void OnMessage(InsteonMessage message)
        {
            var cmd2 = (byte)message.Properties[PropertyKey.Cmd2];

            if (!message.Properties.ContainsKey(PropertyKey.Group))
            {
                base.OnMessage(message);
                return;
            }

            var group = (byte)message.Properties[PropertyKey.Group];

            if (group == 1)
            {
                if (message.MessageType == InsteonMessageType.OnCleanup)
                {
                    logger.Debug("Motion detected");
                    OnDeviceStatusChanged(InsteonDeviceStatus.On);
                }
                if (message.MessageType == InsteonMessageType.OffCleanup)
                {
                    logger.Debug("Motion timed out");
                    OnDeviceStatusChanged(InsteonDeviceStatus.Off);
                }
            }
            else if (group == 2)
            {
                logger.Debug("Light detected");
                OnDeviceStatusChanged(InsteonDeviceStatus.LightDetected);
            }
            else if (group == 3)
            {
                logger.Debug("Low Battery");
                OnDeviceStatusChanged(InsteonDeviceStatus.LowBattery);
                LowBattery = true;
            }
            else
            {
                base.OnMessage(message);
            }
        }*/


    }
}