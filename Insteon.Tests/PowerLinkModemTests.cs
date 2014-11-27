using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Insteon.Network;
using Insteon.Network.Device;
using Insteon.Network.Enum;
using NUnit.Framework;

namespace Insteon.Tests
{
    [TestFixture]
    public class PowerLinkModemTests
    {
        [Test]
        public void SimpleConnectTest()
        {
            var connection = new InsteonConnection(InsteonConnectionType.Serial, "COM5");
            var network = new InsteonNetwork();
            var connected = network.TryConnect(connection);
            network.Controller.DeviceLinked += ControllerOnDeviceLinked;
            network.Controller.GetLinks();
            
            Assert.IsTrue(connected);

            network.AutoAdd = true;

            while (true)
            {
                Thread.Sleep(500);
            }

            network.Close();
        }

        private void ControllerOnDeviceLinked(object sender, InsteonDeviceEventArgs data)
        {
            
        }
    }
}
