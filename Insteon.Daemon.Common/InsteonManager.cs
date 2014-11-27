using System;
using Insteon.Network;
using Insteon.Network.Device;
using ServiceStack.Logging;

namespace Insteon.Daemon.Common
{
    public sealed class InsteonManager
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(InsteonManager));
        private readonly Uri smartAppUri;
        
        public InsteonConnection Connection { get; private set; }
        public InsteonNetwork Network { get; private set; }
        public InsteonManager(string insteonSource, Uri smartAppUri)
        {
            this.smartAppUri = smartAppUri;
            InsteonConnection iConnection;
            if (InsteonConnection.TryParse(insteonSource, out iConnection))
            {
                Connection = iConnection;
            }
            else
            {
                throw new Exception("Could not create Insteon Connection type from " + insteonSource);
            }

            Network = new InsteonNetwork();
        }

        public bool Connect()
        {
            var connected = Network.TryConnect(Connection);

            if (connected)
            {
                Network.Devices.DeviceStatusChanged += OnDeviceStatusChanged;
                Network.Devices.DeviceCommandTimeout += OnDeviceCommandTimeout;
                RefreshDeviceDatabase();
            }
            return connected;
        }

        private void OnDeviceCommandTimeout(object sender, InsteonDeviceEventArgs data)
        {
            logger.Debug("device command timeout");  
        }

        private void OnDeviceStatusChanged(object sender, InsteonDeviceStatusChangedEventArgs data)
        {
           
            logger.Debug("device status changed " + data.Device.ToString());
        }

        private void RefreshDeviceDatabase()
        {
            var links = Network.Controller.GetLinks();

            foreach (var insteonDeviceLinkRecord in links)
            {
                InsteonIdentity id;
                if (Network.Controller.TryGetLinkIdentity(insteonDeviceLinkRecord, out id))
                {
                    var d = Network.Devices.Add(insteonDeviceLinkRecord.Address, id);
                    logger.Debug(string.Format("New device identified and added to device list. ({0})",d));
                }
                else
                {
                    logger.Error("device didn't respond");
                }
            }

        }

    }
}
