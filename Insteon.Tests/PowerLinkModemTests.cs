using System.Configuration;
using System.Linq;
using Insteon.Network;
using Insteon.Network.Device;
using Insteon.Network.Enum;
using NUnit.Framework;
using ServiceStack.Logging;

namespace Insteon.Tests
{
    [TestFixture]
    public class PowerLinkModemTests
    {
        private string insteonSource;
        private InsteonNetwork network;

        [TestFixtureSetUp]
        public void Setup()
        {
            LogManager.LogFactory = new ConsoleLogFactory();
            insteonSource = ConfigurationManager.AppSettings["insteonConnection"];
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            if (network != null && network.IsConnected)
                network.Close();
            
            network = null;
        }
        
        [Test]
        public void SimpleConnectKnownSerialPortTest()
        {
            InsteonConnection connection;
            Assert.IsTrue(InsteonConnection.TryParse(insteonSource, out connection));
            network = new InsteonNetwork();
            var connected = network.TryConnect(connection);

            Assert.IsTrue(connected);
            Assert.AreEqual(connection.Address.Value, InsteonAddress.Parse(ConfigurationManager.AppSettings["plmIdentityTest"]).Value);
            network.Close();
        }

        [Test]
        public void SimpleConnectAnySerialPortTest()
        {
            network = new InsteonNetwork();
            var connected = network.TryConnect();
            
            Assert.IsTrue(connected);
            Assert.AreEqual(network.Connection.Address.Value, InsteonAddress.Parse(ConfigurationManager.AppSettings["plmIdentityTest"]).Value);
            network.Close();
        }

        [Test]
        public void VerifyInsteonNetworkTest()
        {
            network = new InsteonNetwork();
            var connected = network.TryConnect();

            Assert.IsTrue(connected);
            Assert.IsTrue(network.VerifyConnection());
            network.Close();
        }
        
        [Test]
        public void GetLinksTest()
        {
            network = new InsteonNetwork();
            var connected = network.TryConnect();
            Assert.IsTrue(connected);
            var links = network.Controller.GetLinks();

            Assert.IsNotEmpty(links);
            network.Close();
        }


        [Test]
        public void GetLinkIdentityTest()
        {
            network = new InsteonNetwork();
            var connected = network.TryConnect();
            Assert.IsTrue(connected);
            var links = network.Controller.GetLinks();

            Assert.IsNotEmpty(links);

            var responders = links.Where(l => l.RecordType == InsteonDeviceLinkRecordType.Responder);
           

            foreach (var insteonDeviceLinkRecord in responders)
            {
                
                if (network.Devices.ContainsKey(insteonDeviceLinkRecord.Address))
                    continue;
                
                InsteonIdentity? id;
                if (network.Controller.TryGetLinkIdentity(insteonDeviceLinkRecord, out id))
                {
                    if (id != null)
                    {
                        var d = network.Devices.Add(insteonDeviceLinkRecord.Address, id.Value);

                    }
                }
                else
                {
                   Assert.Ignore("Possibly a battery powered device.");
                }
            }

            network.Close();
        }

    }
}
