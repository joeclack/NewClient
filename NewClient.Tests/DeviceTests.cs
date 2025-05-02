using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using NewClient.Interfaces;
using NewClient.Factories;
using NewClient.Devices;
using System.Net.Http;
using System.Text.Json;
using FluentAssertions;
using IHttpClientFactory = NewClient.Interfaces.IHttpClientFactory;
using NewClient.Models;

namespace NewClient.Tests
{
    [TestClass]
    public class DeviceTests
    {
        private Mock<IHttpClientFactory>? _mockHttpClientFactory;
        private Mock<ILogger>? _mockLogger;
        private DeviceFactory? _deviceFactory;
        private HttpClient? _httpClient;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger>();
            _httpClient = new HttpClient();
            _mockHttpClientFactory.Setup(x => x.CreateClient()).Returns(_httpClient);
            _deviceFactory = new DeviceFactory(_mockHttpClientFactory.Object, _mockLogger.Object);
        }

        [TestMethod]
        public void CreateDevice_ShouldCreateFanDevice_WhenTypeIsFan()
        {
            
            var device = _deviceFactory.CreateDevice(DeviceType.Fan, 1);

           
            device.Should().BeOfType<FanDevice>();
            device.Id.Should().Be(1);
        }

        [TestMethod]
        public void CreateDevice_ShouldCreateHeaterDevice_WhenTypeIsHeater()
        {
            
            var device = _deviceFactory.CreateDevice(DeviceType.Heater, 1);

           
            device.Should().BeOfType<HeaterDevice>();
            device.Id.Should().Be(1);
        }

        [TestMethod]
        public void CreateDevice_ShouldCreateSensorDevice_WhenTypeIsSensor()
        {
            
            var device = _deviceFactory.CreateDevice(DeviceType.Sensor, 1);

           
            device.Should().BeOfType<SensorDevice>();
            device.Id.Should().Be(1);
        }

        [TestMethod]
        public void CreateDevice_ShouldThrowArgumentException_WhenTypeIsInvalid()
        {
            
            var act = () => _deviceFactory.CreateDevice((DeviceType)999, 1);

           
            act.Should().Throw<ArgumentException>()
                .WithMessage("Unknown device type: 999");
        }

        [TestMethod]
        public void CreateDevice_ShouldSetCorrectId_ForEachDevice()
        {
            
            var fan = _deviceFactory.CreateDevice(DeviceType.Fan, 1);
            var heater = _deviceFactory.CreateDevice(DeviceType.Heater, 2);
            var sensor = _deviceFactory.CreateDevice(DeviceType.Sensor, 3);

           
            fan.Id.Should().Be(1);
            heater.Id.Should().Be(2);
            sensor.Id.Should().Be(3);
        }
    }
} 