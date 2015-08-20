using Insteon.Network.Device;
using Insteon.Network.Enum;
using Insteon.Network.Message;
using ServiceStack.Logging;

namespace Insteon.Network.Devices
{
    public class IoLinc : InsteonDevice
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(IoLinc));

        public IOState SensorStatus { get; set; }
        public IOState RelayStatus { get; set; }

        internal IoLinc(InsteonNetwork network, InsteonAddress address, InsteonIdentity identity)
            : base(network, address, identity)
        {
        }

        internal override void OnMessage(InsteonMessage message)
        {

            if (message.MessageType == InsteonMessageType.OnCleanup)
            {
                SensorStatus = IOState.Closed;
                OnDeviceStatusChanged(InsteonDeviceStatus.SensorTriggerOn);
            }
            else if (message.MessageType == InsteonMessageType.OffCleanup)
            {
                SensorStatus = IOState.Open;
                OnDeviceStatusChanged(InsteonDeviceStatus.SensorTriggerOff);
            }
            else
            {
                base.OnMessage(message);
            }
        }

        public bool UpdateStatus()
        {
            logger.DebugFormat("Updating IOLinc {0} status...", Address);

            byte value;
            if (TryGetStatus(out value, 0x0))
            {
                RelayStatus = value == 0 ? IOState.Open : IOState.Closed;

                if (TryGetStatus(out value, 0x01))
                {
                    SensorStatus = value == 0 ? IOState.Open : IOState.Closed;
                    return true;
                }
            }

            return false;
        }
    }
}
