using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Insteon.Network;
using ServiceStack.Logging;

namespace Insteon.Daemon.Common
{
    public sealed class InsteonManager
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(InsteonManager));
        private readonly Uri smartAppUri;
        private readonly ManualResetEvent signalEvent = new ManualResetEvent(false);
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
                Network.Devices.DeviceIdentified += (s, e) => signalEvent.Set();
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
           
            logger.Debug("device status changed " + data.Device.Address.ToString());
        }

        private void RefreshDeviceDatabase()
        {
            var links = Network.Controller.GetLinks();

            foreach (var insteonDeviceLinkRecord in links)
            {
                var d = Network.Devices.Add(insteonDeviceLinkRecord.Address, new InsteonIdentity());
                // note: this blocks on multiple calls until previous returns. perhaps use event to set wait?
                d.Identify();
                signalEvent.WaitOne(new TimeSpan(0, 0, 0, 5));
                signalEvent.Reset();
                //await TaskExtension.FromEvent<InsteonDeviceEventHandler, InsteonDeviceEventArgs>(
                //    (complete, cancel, reject)=>
                //        (sender, data) =>
                //        {

                //        }, 
                //    handler => d.DeviceIdentified += this.handlerTest ,
                //    handler => d.DeviceIdentified -= this.handlerTest, 
                //    (complete, cancel, reject)=> d.Identify(), 
                //    CancellationToken.None);


            }

        }

    }
}
