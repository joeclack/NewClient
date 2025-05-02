using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewClient.Devices;
using NewClient.Factories;
using NewClient.Interfaces;
using System.Net.Http;
using IHttpClientFactory = NewClient.Interfaces.IHttpClientFactory;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NewClient.Tests")]

namespace NewClient.Controllers
{
	public class EnvironmentController : IEnvironmentController
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<EnvironmentController> _logger;
		public readonly int _numberOfFans;
		public readonly int _numberOfHeaters;
		public readonly int _numberOfSensors;
		public const int MaxHeaterLevel = 3;
		private readonly DeviceFactory _deviceFactory;
		private readonly List<IDevice> _fans;
		private readonly List<IDevice> _heaters;
		private readonly List<IDevice> _sensors;

		internal IReadOnlyList<IDevice> Fans => _fans;
		internal IReadOnlyList<IDevice> Heaters => _heaters;
		internal IReadOnlyList<IDevice> Sensors => _sensors;

		public EnvironmentController(
			IHttpClientFactory httpClientFactory,
			ILogger<EnvironmentController> logger,
			IConfiguration configuration)
		{
			_httpClientFactory = httpClientFactory;
			_logger            = logger;
			_numberOfFans      = configuration.GetValue("Environment:NumberOfFans", 3);
			_numberOfHeaters   = configuration.GetValue("Environment:NumberOfHeaters", 3);
			_numberOfSensors   = configuration.GetValue("Environment:NumberOfSensors", 3);
			_deviceFactory     = new DeviceFactory(_httpClientFactory, logger);

			_fans              = [];
			_heaters           = [];
			_sensors           = [];
			SetDevices();
		}


		// What I am doing here is basically creating all the devices based
		// off the amount of devices in the config. 
			
		// i suppose there could be sensors that arent within the amount, so if 3 are 
		// specified in the config, but someone knew that there was a 4th one,
		// i dont think that would work, so they would have to sat 4 in the config
		public void SetDevices()
		{
			foreach(var type in Enum.GetValues(typeof(DeviceType)))
			{
				switch ( type )
				{
					case DeviceType.Fan:
						for ( int i = 1; i <= _numberOfFans; i++ )
						{
							_fans.Add(_deviceFactory.CreateDevice(DeviceType.Fan, i));
						}
						break;
					case DeviceType.Sensor:
						for ( int i = 1; i <= _numberOfSensors; i++ )
						{
							_sensors.Add(_deviceFactory.CreateDevice(DeviceType.Sensor, i));
						}
						break;
					case DeviceType.Heater:
						for ( int i = 1; i <= _numberOfHeaters; i++ )
						{
							_heaters.Add(_deviceFactory.CreateDevice(DeviceType.Heater, i));
						}
						break;
				}
			}
		}

		public async Task SetFanState(int fanId, bool isOn)
		{
			if (fanId < 1 || fanId > _numberOfFans)
			{
				throw new ArgumentOutOfRangeException(nameof(fanId), $"Fan ID must be between 1 and {_numberOfFans}");
			}

			await _fans[fanId - 1].SetState(isOn);
			Console.WriteLine($"Fan {fanId} has been turned {(isOn ? "On" : "Off")}");
		}

		public async Task SetHeaterLevel(int heaterId, int level)
		{
			if (heaterId < 1 || heaterId > _numberOfHeaters)
			{
				throw new ArgumentOutOfRangeException(nameof(heaterId), $"Heater ID must be between 1 and {_numberOfHeaters}");
			}
			if (level < 0 || level > MaxHeaterLevel)
			{
				throw new ArgumentOutOfRangeException(nameof(level), $"Heater level must be between 0 and {MaxHeaterLevel}");
			}

			await _heaters[heaterId - 1].SetLevel(level);
			_logger.LogInformation("Heater {HeaterId} level set to {Level}", heaterId, level);
		}

		public async Task<DeviceStateResult> GetFanState(int id)
		{
			if (id < 1 || id > _numberOfFans)
			{
				throw new ArgumentOutOfRangeException(nameof(id), $"Fan ID must be between 1 and {_numberOfFans}");
			}

			var result = await _fans[id - 1].GetState();
			if (!result.HasError)
			{
				Console.WriteLine($"Fan {id} is {(result.IsOn ? "On" : "Off")}");
			}
			return result;
		}

		public async Task<int> GetHeaterLevel(int id)
		{
			if (id < 1 || id > _numberOfHeaters)
			{
				throw new ArgumentOutOfRangeException(nameof(id), $"Heater ID must be between 1 and {_numberOfHeaters}");
			}

			var level = await _heaters[id - 1].GetLevel();
			Console.WriteLine($"Heater {id} is at level {level}");
			return level;
		}

		public async Task<double> GetSensorTemperature(int sensorId)
		{
			var temp = await _sensors[sensorId - 1].GetTemperature();
			Console.WriteLine($"Sensor {sensorId} temperature: {temp}°C");
			return temp;
		}

		public async Task<string> GetSensor1Temperature()
		{
			var temp = await _sensors[0].GetTemperature();
			return temp.ToString();
		}

		public async Task<int> GetSensor2Temperature()
		{
			var temp = await _sensors[1].GetTemperature();
			return (int)temp;
		}

		public async Task<decimal> GetSensor3Temperature()
		{
			var temp = await _sensors[2].GetTemperature();
			return (decimal)temp;
		}

		public async Task<double> GetAverageTemperature()
		{
			try
			{
				// kinda messy having to parse and cast all these different types
				
				var sensor1 = double.Parse(await GetSensor1Temperature());
				var sensor2 = await GetSensor2Temperature();
				var sensor3 = (double)await GetSensor3Temperature();

				double avgTemperature = (sensor1 + sensor2 + sensor3) / 3;
				_logger.LogInformation("Average temperature: {Temperature}°C", avgTemperature);
				return avgTemperature;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				_logger.LogError("Failed to calculate average temperature");
				return 0;
			}
		}

		public async Task SetAllHeaters(int level)
		{
			if (level < 0 || level > MaxHeaterLevel)
			{
				throw new ArgumentOutOfRangeException(nameof(level), $"Heater level must be between 0 and {MaxHeaterLevel}");
			}

			try
			{
				foreach (var heater in _heaters)
				{
					await heater.SetLevel(level);
				}
				Console.WriteLine($"All heaters have been set to level {level}");
				_logger.LogInformation("All heaters set to level {Level}", level);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				_logger.LogError("Failed to set all heaters to level {Level}", level);
			}
		}

		public async Task SetAllFans(bool state)
		{
			try
			{
				foreach (var fan in _fans)
				{
					await fan.SetState(state);
				}
				Console.WriteLine($"All fans have been turned {(state ? "On" : "Off")}");
				_logger.LogInformation("All fans turned {State}", state ? "On" : "Off");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				_logger.LogError("Failed to set all fans to {State}", state);
			}
		}

		public async Task GetAllStates()
		{
			try
			{
				Console.WriteLine("\nFetching fan states individually...");
				_logger.LogInformation("Fetching fan states individually...");
				for (int i = 0; i < _fans.Count; i++)
				{
					var fanState = await _fans[i].GetState();
					if (fanState.HasError)
					{
						Console.WriteLine($"  Fan {i + 1}: Error - {fanState.ErrorMessage}");
						_logger.LogError("Fan {FanId}: Error - {ErrorMessage}", i + 1, fanState.ErrorMessage);
					}
					else
					{
						Console.WriteLine($"  Fan {i + 1}: {(fanState.IsOn ? "On" : "Off")}");
						_logger.LogInformation("Fan {FanId}: {State}", i + 1, fanState.IsOn ? "On" : "Off");
					}
				}

				Console.WriteLine("\nFetching heater levels individually...");
				_logger.LogInformation("Fetching heater levels individually...");
				for (int i = 0; i < _heaters.Count; i++)
				{
					var heaterLevel = await _heaters[i].GetLevel();
					Console.WriteLine($"  Heater {i + 1}: Level {heaterLevel}");
					_logger.LogInformation("Heater {HeaterId}: Level {Level}", i + 1, heaterLevel);
				}

				Console.WriteLine("\nFetching sensor temperatures individually...");
				_logger.LogInformation("Fetching sensor temperatures individually...");
				for (int i = 0; i < _sensors.Count; i++)
				{
					var temperature = await _sensors[i].GetTemperature();
					Console.WriteLine($"  Sensor {i + 1}: Temperature {temperature}°C");
					_logger.LogInformation("Sensor {SensorId}: Temperature {Temperature}°C", i + 1, temperature);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				_logger.LogError("Error fetching device states");
			}
		}

		public async Task<double> HoldTemperature(double currentTemperature, double targetTemperature, int durationSeconds)
		{
			try
			{
				// this is pretty basic temp control - just on/off for heaters and fans
				// could make it smarter by adjusting heater levels based on how far off we are
				Console.WriteLine($"Holding temperature at {targetTemperature}°C for {durationSeconds} seconds...");
				_logger.LogInformation("Holding temperature at {TargetTemperature}°C for {Duration} seconds", targetTemperature, durationSeconds);
				int intervalMs = 1000; // 1-second intervals

				for (int i = 0; i < durationSeconds; i++)
				{
					if (currentTemperature < targetTemperature)
					{
						await SetAllHeaters(1); // Minimal heating
						await SetAllFans(false); // Reduce cooling
					}
					else if (currentTemperature > targetTemperature)
					{
						await SetAllHeaters(0); // Turn off heating
						await SetAllFans(true); // Activate cooling
					}

					await Task.Delay(intervalMs);
					currentTemperature = await GetAverageTemperature();
					Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");
					_logger.LogInformation("Current temperature: {Temperature}°C", currentTemperature);
				}

				return currentTemperature;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				_logger.LogError("Failed to hold temperature at {TargetTemperature}°C", targetTemperature);
				return currentTemperature;
			}
		}

		public async Task<double> AdjustTemperature(double currentTemperature, double targetTemperature, int durationSeconds)
		{
			try
			{
				// left as is. not required to change this logic, apart from tydying it up
				Console.WriteLine($"Adjusting temperature to {targetTemperature}°C over {durationSeconds} seconds...");
				_logger.LogInformation("Adjusting temperature to {TargetTemperature}°C over {Duration} seconds", targetTemperature, durationSeconds);
				int intervalMs = 1000; // 1-second intervals
				int iterations = durationSeconds;

				for (int i = 0; i < iterations; i++)
				{
					if (Math.Abs(currentTemperature - targetTemperature) <= 0.1) break;

					if (currentTemperature < targetTemperature)
					{
						await SetAllHeaters(3); // Set heaters to level 3
						await SetAllFans(false); // Turn off fans
					}
					else
					{
						await SetAllHeaters(0); // Turn off heaters
						await SetAllFans(true); // Turn on fans
					}

					await Task.Delay(intervalMs);
					currentTemperature = await GetAverageTemperature();
					Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");
					_logger.LogInformation("Current temperature: {Temperature}°C", currentTemperature);
				}

				return currentTemperature;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				_logger.LogError("Failed to adjust temperature to {TargetTemperature}°C", targetTemperature);
				return currentTemperature;
			}
		}

		public async Task<HttpResponseMessage> ResetEnvironment()
		{
			try
			{
				// simple reset endpoint - just tells the api to reset everything
				// might want to add some kind of confirmation or safety check here
				var response = await _httpClientFactory.CreateClient().PostAsync("api/Envo/reset", null);
				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Failed to reset environment: {Reason}", response.ReasonPhrase);
					Console.WriteLine($"Error: Failed to reset environment: {response.ReasonPhrase}");
					return response;
				}
				_logger.LogInformation("Environment reset successfully");
				return response;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				_logger.LogError("Failed to reset environment");
				return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
			}
		}
	}
}
