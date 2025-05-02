using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewClient.Controllers;
using NewClient.Interfaces;
using NewClient.Devices;
using System;
using System.Threading.Tasks;
using IHttpClientFactory = NewClient.Interfaces.IHttpClientFactory;
using Microsoft.Testing.Platform.Configurations;
using Microsoft.Testing.Platform.Logging;
using FluentAssertions;
using System.Net.Http;
using System.Threading;
using System.Net;
using Moq.Protected;

namespace NewClient.Tests
{
	[TestClass]
	public class EnvironmentControllerTests
	{
		private Mock<IHttpClientFactory>? _mockHttpClientFactory;
		private Mock<Microsoft.Extensions.Logging.ILogger<EnvironmentController>>? _mockLogger;
		private Mock<Microsoft.Extensions.Configuration.IConfiguration>? _mockConfiguration;
		private EnvironmentController? _controller;

		[TestInitialize]
		public void TestInitialize()
		{
			// setup a bunch of mock stuff!!!!

			_mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
			var configSection = new Mock<IConfigurationSection>();
			configSection.Setup(x => x.Value).Returns("3"); // Set number of fans to 3
			_mockConfiguration.Setup(x => x.GetSection("Environment:NumberOfFans")).Returns(configSection.Object);
			_mockConfiguration.Setup(x => x.GetSection("Environment:NumberOfHeaters")).Returns(configSection.Object);
			_mockConfiguration.Setup(x => x.GetSection("Environment:NumberOfSensors")).Returns(configSection.Object);

			_mockHttpClientFactory = new Mock<IHttpClientFactory>();
			var mockHttpClient = new Mock<System.Net.Http.HttpClient>();
			mockHttpClient.Object.BaseAddress = new Uri("http://localhost");
			_mockHttpClientFactory.Setup(x => x.CreateClient()).Returns(mockHttpClient.Object);

			_mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<EnvironmentController>>();

			_controller = new EnvironmentController(
				_mockHttpClientFactory.Object,
				_mockLogger.Object,
				_mockConfiguration.Object
			);

		}

		[TestMethod]
		[DataRow(0, false)]  // Below valid range
		[DataRow(4, true)]   // Above valid range
		public async Task SetFanState_ShouldThrowArgumentOutOfRangeException_WhenFanIdIsInvalid(int fanId, bool isOn)
		{
			
			var act = () => _controller.SetFanState(fanId, isOn);

			// Assert
			await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
				.WithMessage($"Fan ID must be between 1 and {_controller._numberOfFans}*");
		}

		[TestMethod]
		[DataRow(0, 2)]  // Invalid heater ID
		[DataRow(4, 2)]  // Invalid heater ID
		[DataRow(1, -1)] // Invalid level
		[DataRow(1, 4)]  // Invalid level
		public async Task SetHeaterLevel_ShouldThrowArgumentOutOfRangeException_WhenInputIsInvalid(int heaterId, int level)
		{
			
			var act = () => _controller.SetHeaterLevel(heaterId, level);

			// Assert
			await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
		}

		[TestMethod]
		[DataRow(0)]  // Invalid sensor ID
		[DataRow(4)]  // Invalid sensor ID
		public async Task GetSensorTemperature_ShouldThrowException_WhenSensorIdIsInvalid(int sensorId)
		{
			
			await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() =>
				_controller.GetSensorTemperature(sensorId));
		}

		[TestMethod]
		[DataRow(-1)]  // Invalid level
		[DataRow(4)]   // Invalid level
		public async Task SetAllHeaters_ShouldThrowArgumentOutOfRangeException_WhenLevelIsInvalid(int level)
		{
			
			await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() =>
				_controller.SetAllHeaters(level));
		}

		[TestMethod]
		public async Task GetAverageTemperature_ShouldReturnZero_WhenCalculationFails()
		{
			
			var result = await _controller.GetAverageTemperature();

			// Assert
			result.Should().Be(0);
		}

		[TestMethod]
		public async Task SetAllFans_ShouldNotThrowException_WhenCalled()
		{
			
			try
			{
				await _controller.SetAllFans(true);
				Assert.IsTrue(true);
			}
			catch (Exception ex)
			{
				Assert.Fail($"Expected no exception, but got: {ex.Message}");
			}
		}

		[TestMethod]
		public async Task GetFanState_ShouldThrowArgumentOutOfRangeException_WhenFanIdIsInvalid()
		{
			
			var act = () => _controller.GetFanState(0);

			// Assert
			await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
				.WithMessage($"Fan ID must be between 1 and {_controller._numberOfFans}*");
		}
	}
}