using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NewClient.Controllers;
using NewClient.Interfaces;
using System.Net;
using System.Net.Http;
using Xunit;

namespace NewClient.Tests
{
    public class EnvironmentControllerTests
    {
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly Mock<ILogger<EnvironmentController>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly EnvironmentController _controller;

        public EnvironmentControllerTests()
        {
            _mockHttpClient = new Mock<HttpClient>();
            _mockLogger = new Mock<ILogger<EnvironmentController>>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Setup configuration
            _mockConfiguration.Setup(x => x.GetValue<int>("Environment:NumberOfFans", It.IsAny<int>()))
                .Returns(3);
            _mockConfiguration.Setup(x => x.GetValue<int>("Environment:NumberOfHeaters", It.IsAny<int>()))
                .Returns(3);

            _controller = new EnvironmentController(
                _mockHttpClient.Object,
                _mockLogger.Object,
                _mockConfiguration.Object
            );
        }

        [Fact]
        public async Task SetFanState_ValidFanId_ShouldSucceed()
        {
            // Arrange
            int fanId = 1;
            bool expectedState = true;

            // Act
            await _controller.SetFanState(fanId, expectedState);

            // Assert
            // Verify the fan state was set correctly
            var actualState = await _controller.GetFanState(fanId);
            Assert.Equal(expectedState, actualState);
        }

        [Fact]
        public async Task SetFanState_InvalidFanId_ShouldThrowException()
        {
            // Arrange
            int invalidFanId = 4; // Assuming we have 3 fans

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _controller.SetFanState(invalidFanId, true)
            );
        }

        [Fact]
        public async Task SetHeaterLevel_ValidLevel_ShouldSucceed()
        {
            // Arrange
            int heaterId = 1;
            int expectedLevel = 2;

            // Act
            await _controller.SetHeaterLevel(heaterId, expectedLevel);

            // Assert
            var actualLevel = await _controller.GetHeaterLevel(heaterId);
            Assert.Equal(expectedLevel, actualLevel);
        }

        [Fact]
        public async Task SetHeaterLevel_InvalidLevel_ShouldThrowException()
        {
            // Arrange
            int heaterId = 1;
            int invalidLevel = 4; // Max level is 3

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _controller.SetHeaterLevel(heaterId, invalidLevel)
            );
        }

        [Fact]
        public async Task GetAverageTemperature_ShouldReturnCorrectAverage()
        {
            // Arrange
            double expectedAvg = 25.0;

            // Act
            var actualAvg = await _controller.GetAverageTemperature();

            // Assert
            Assert.Equal(expectedAvg, actualAvg, 1); // Allow for small floating point differences
        }

        [Fact]
        public async Task SetAllHeaters_ShouldSetAllHeatersToSameLevel()
        {
            // Arrange
            int expectedLevel = 2;

            // Act
            await _controller.SetAllHeaters(expectedLevel);

            // Assert
            for (int i = 1; i <= 3; i++)
            {
                var level = await _controller.GetHeaterLevel(i);
                Assert.Equal(expectedLevel, level);
            }
        }

        [Fact]
        public async Task SetAllFans_ShouldSetAllFansToSameState()
        {
            // Arrange
            bool expectedState = true;

            // Act
            await _controller.SetAllFans(expectedState);

            // Assert
            for (int i = 1; i <= 3; i++)
            {
                var state = await _controller.GetFanState(i);
                Assert.Equal(expectedState, state);
            }
        }

        [Fact]
        public async Task HoldTemperature_ShouldMaintainTemperature()
        {
            // Arrange
            double currentTemp = 20.0;
            double targetTemp = 25.0;
            int durationSeconds = 5;

            // Act
            var finalTemp = await _controller.HoldTemperature(currentTemp, targetTemp, durationSeconds);

            // Assert
            Assert.InRange(finalTemp, targetTemp - 1, targetTemp + 1);
        }

        [Fact]
        public async Task AdjustTemperature_ShouldReachTargetTemperature()
        {
            // Arrange
            double currentTemp = 20.0;
            double targetTemp = 25.0;
            int durationSeconds = 5;

            // Act
            var finalTemp = await _controller.AdjustTemperature(currentTemp, targetTemp, durationSeconds);

            // Assert
            Assert.InRange(finalTemp, targetTemp - 1, targetTemp + 1);
        }

        [Fact]
        public async Task ResetEnvironment_ShouldSucceed()
        {
            // Arrange
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            _mockHttpClient.Setup(x => x.PostAsync("api/Envo/reset", null))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _controller.ResetEnvironment();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
} 