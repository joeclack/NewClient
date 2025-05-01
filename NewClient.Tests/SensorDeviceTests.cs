using Microsoft.Extensions.Logging;
using Moq;
using NewClient.Devices;
using System.Net;
using System.Net.Http;
using Xunit;

namespace NewClient.Tests
{
    public class SensorDeviceTests
    {
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly Mock<ILogger> _mockLogger;
        private readonly SensorDevice _sensor;

        public SensorDeviceTests()
        {
            _mockHttpClient = new Mock<HttpClient>();
            _mockLogger = new Mock<ILogger>();
            _sensor = new SensorDevice(_mockHttpClient.Object, _mockLogger.Object, 1);
        }

        [Fact]
        public async Task GetTemperature_ValidResponse_ShouldReturnTemperature()
        {
            // Arrange
            var expectedTemperature = 25.5;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(expectedTemperature.ToString())
            };
            _mockHttpClient.Setup(x => x.GetAsync("api/Sensor/sensor1"))
                .ReturnsAsync(response);

            // Act
            var actualTemperature = await _sensor.GetTemperature();

            // Assert
            Assert.Equal(expectedTemperature, actualTemperature);
        }

        [Fact]
        public async Task GetTemperature_InvalidResponse_ShouldReturnZero()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockHttpClient.Setup(x => x.GetAsync("api/Sensor/sensor1"))
                .ReturnsAsync(response);

            // Act
            var temperature = await _sensor.GetTemperature();

            // Assert
            Assert.Equal(0, temperature);
        }

        [Fact]
        public async Task GetState_TemperatureAboveZero_ShouldReturnTrue()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("25.5")
            };
            _mockHttpClient.Setup(x => x.GetAsync("api/Sensor/sensor1"))
                .ReturnsAsync(response);

            // Act
            var state = await _sensor.GetState();

            // Assert
            Assert.True(state);
        }

        [Fact]
        public async Task GetState_TemperatureZero_ShouldReturnFalse()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("0")
            };
            _mockHttpClient.Setup(x => x.GetAsync("api/Sensor/sensor1"))
                .ReturnsAsync(response);

            // Act
            var state = await _sensor.GetState();

            // Assert
            Assert.False(state);
        }

        [Fact]
        public async Task GetLevel_ShouldAlwaysReturnZero()
        {
            // Act
            var level = await _sensor.GetLevel();

            // Assert
            Assert.Equal(0, level);
        }

        [Fact]
        public async Task SetLevel_ShouldCompleteWithoutError()
        {
            // Act & Assert
            await _sensor.SetLevel(1); // Should not throw any exception
        }

        [Fact]
        public async Task SetState_ShouldCompleteWithoutError()
        {
            // Act & Assert
            await _sensor.SetState(true); // Should not throw any exception
        }
    }
} 