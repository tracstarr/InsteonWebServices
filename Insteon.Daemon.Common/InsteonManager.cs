using System;
using System.Linq;
using Insteon.Data;
using Insteon.Network;
using Insteon.Network.Device;
using ServiceStack.Logging;

namespace Insteon.Daemon.Common
{
    public sealed class InsteonManager
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(InsteonManager));

        public InsteonConnection Connection { get; private set; }
        public InsteonNetwork Network { get; private set; }
        public InsteonManager(string insteonSource)
        {
            logger.Debug("Creating insteon manager");
            InsteonConnection iConnection;
            if (InsteonConnection.TryParse(insteonSource, out iConnection))
            {
                logger.DebugFormat("Parsed InsteonConnection to {0}", iConnection.ToString());
                Connection = iConnection;
            }
            else
            {
                throw new Exception("Could not create Insteon Connection type from " + insteonSource);
            }

            Network = new InsteonNetwork { AutoAdd = true };
        }

        public bool Connect()
        {
            logger.Debug("Trying to connect to insteon controller.");
            var connected = Network.TryConnect(Connection);

            if (connected)
            {
                Network.Devices.DeviceStatusChanged += OnDeviceStatusChanged;
                Network.Devices.DeviceCommandTimeout += OnDeviceCommandTimeout;
                Network.Devices.DeviceIdentified += DevicesOnDeviceIdentified;
                Network.Controller.DeviceLinked += OnDeviceLinked;
                LoadDevicesFromDatabase();
                Network.Devices.DeviceAdded += DevicesOnDeviceAdded;
            }
            return connected;
        }

        private void DevicesOnDeviceAdded(object sender, InsteonDeviceEventArgs data)
        {
            logger.Debug("Device added.");
            
            var dataManager = new InsteonDataManager(false);
            var found = dataManager.GetByAddress(data.Device.Address.ToString());
            if (found != null)
            {
                // update
                found.Category = data.Device.Identity.DevCat;
                found.SubCategory = data.Device.Identity.SubCat;
                found.Firmware = data.Device.Identity.FirmwareVersion;
                found.ProductKey = data.Device.Identity.ProductKey?.StringKey();
                dataManager.Update(found);
            }
            else
            {
                // insert
                dataManager.Add(new InsteonDeviceModel()
                {
                    Address = data.Device.Address.ToString(),
                    Category = data.Device.Identity.DevCat,
                    SubCategory = data.Device.Identity.SubCat,
                    Firmware = data.Device.Identity.FirmwareVersion,
                    ProductKey = data.Device.Identity.ProductKey?.StringKey()
                });
            }
        }

        private void DevicesOnDeviceIdentified(object sender, InsteonDeviceEventArgs data)
        {
            logger.InfoFormat("Device Identified {0}", data.Device.Address.ToString());
        }

        private void LoadDevicesFromDatabase()
        {
            var dataManager = new InsteonDataManager(false);
            var devices = dataManager.GetAllDevices();

            foreach (var device in devices)
            {
                var id = new InsteonIdentity(device.Category, device.SubCategory, device.Firmware);
                if (!id.IsEmpty)
                    Network.Devices.Add(InsteonAddress.Parse(device.Address), id);
            }
        }

        private void OnDeviceLinked(object sender, InsteonDeviceEventArgs data)
        {
            logger.Debug("New device found and linked");
        }

        private void OnDeviceCommandTimeout(object sender, InsteonDeviceEventArgs data)
        {
            logger.Debug("device command timeout");
        }

        private void OnDeviceStatusChanged(object sender, InsteonDeviceStatusChangedEventArgs data)
        {

            logger.DebugFormat("device status changed {0} [{1}]", data.Device.ToString(), data.DeviceStatus);
        }

        public void RefreshDeviceDatabase()
        {
            var dataManager = new InsteonDataManager(false);

            Network.Devices.DeviceAdded -= DevicesOnDeviceAdded;

            var allLinks = Network.Controller.GetDeviceLinkRecords(true);
            var insteonAddresses = allLinks.Select(l => l.Address).Distinct();

            // TODO: modify this so that we can call refresh when devices exist, and update netowrk items and db as necessary

            foreach (var insteonAddress in insteonAddresses)
            {
                if (Network.Devices.ContainsKey(insteonAddress))
                    continue;

                if (dataManager.GetByAddress(insteonAddress.ToString()) != null)
                    continue;

                InsteonIdentity? id;
                if (Network.Controller.TryGetLinkIdentity(insteonAddress, out id))
                {
                    if (id != null)
                    {
                        var d = Network.Devices.Add(insteonAddress, id.Value);

                        dataManager.Add(new InsteonDeviceModel()
                        {
                            Address = d.Address.ToString(),
                            Category = id.Value.DevCat,
                            SubCategory = id.Value.SubCat,
                            Firmware = id.Value.FirmwareVersion,
                            ProductKey = id.Value.ProductKey?.ToString(),

                        });

                        logger.DebugFormat("New device identified and added to device list. ({0})", d);
                    }
                    else
                    {
                        logger.Debug("What does this mean?");
                    }
                }
                else
                {
                    dataManager.Add(new InsteonDeviceModel()
                    {
                        Address = insteonAddress.ToString(),
                        Category = 0,
                        SubCategory = 0,
                        Firmware = 0
                    });

                    logger.Warn("device didn't respond. Battery powered?");
                }
            }

            Network.Devices.DeviceAdded += DevicesOnDeviceAdded;
        }

    }
}
