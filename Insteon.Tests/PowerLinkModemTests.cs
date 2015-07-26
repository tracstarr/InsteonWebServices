using System;
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
            var links = network.Controller.GetDeviceLinkRecords();

            foreach (var link in links)
            {
                Console.WriteLine(link.Address + ":" + link.RecordType);
            }

            Assert.IsNotEmpty(links);
            network.Close();
        }


        [Test]
        public void GetLinkIdentityTest()
        {
            network = new InsteonNetwork();
            var connected = network.TryConnect();
            Assert.IsTrue(connected);
            var links = network.Controller.GetDeviceLinkRecords();

            Assert.IsNotEmpty(links);


            var insteonAddresses = links.Select(l => l.Address).Distinct();


            foreach (var insteonAddress in insteonAddresses)
            {

                if (network.Devices.ContainsKey(insteonAddress))
                    continue;

                InsteonIdentity? id;
                if (network.Controller.TryGetLinkIdentity(insteonAddress, out id))
                {
                    if (id != null)
                    {
                        var d = network.Devices.Add(insteonAddress, id.Value);

                    }
                }
                else
                {
                    Console.WriteLine("Possibly a battery powered device.");
                }
            }

            network.Close();
        }

    }
}
