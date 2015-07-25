using System;
using Insteon.Data;
using NUnit.Framework;
using FluentAssertions;

namespace Insteon.Tests
{
    [TestFixture]
    public class DataTests
    {
        [Test]
        public void SimpleExistsTest()
        {
            var manager = new InsteonDataManager(true);
            var devices = manager.GetAllDevices();
            Assert.That(devices.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddDeviceRecordTest()
        {
            var manager = new InsteonDataManager(true);
            var id = manager.Add(new InsteonDeviceModel()
            {
                Address = "34.55.66",
                Category = 0x01,
                SubCategory = 0x02,
                DisplayName = "Test Device",
                Firmware = 0x45
            });

            id.Should().Be(1);
        }

        [Test]
        public void DuplicateAddressFailTest()
        {
            var manager = new InsteonDataManager(true);
            var id = manager.Add(new InsteonDeviceModel()
            {
                Address = "34.55.66",
                Category = 0x01,
                SubCategory = 0x02,
                DisplayName = "Test Device",
                Firmware = 0x45
            });

            id.Should().Be(1);

            Action act = () =>
            manager.Add(new InsteonDeviceModel()
            {
                Address = "34.55.66",
                Category = 0x01,
                SubCategory = 0x02,
                DisplayName = "Test Device 2",
                Firmware = 0x45
            });

            act.ShouldThrow<Exception>();
        }

        [Test]
        public void UpdateDeviceTest()
        {
            var manager = new InsteonDataManager(true);
            var device = new InsteonDeviceModel()
            {
                Address = "34.55.66",
                Category = 0x01,
                SubCategory = 0x02,
                DisplayName = "Test Device",
                Firmware = 0x45
            };

            var id = manager.Add(device);
            id.Should().Be(1);

            device.Id = 1;

            var d = manager.GetByAddress(device.Address);
            d.Should().NotBeNull();
            d.Address.Should().Be(device.Address);
            d.Category.Should().Be(device.Category);

            device.Category = 0x04;
            manager.Update(device);

            device.Category.Should().Be(0x04);
            d = manager.GetByAddress(device.Address);
            d.Address.Should().Be(device.Address);
            d.Category.Should().Be(device.Category);

        }
    }
}