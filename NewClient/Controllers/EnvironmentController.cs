using NewClient.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace NewClient.Controllers
{
	public class EnvironmentController : IEnvironmentController
	{
		private readonly HttpClient _client;
		private readonly ILogger<EnvironmentController> _logger;
		private readonly int _numberOfFans;
		private readonly int _numberOfHeaters;
		private const int MaxHeaterLevel = 3;

		public EnvironmentController(
			HttpClient client,
			ILogger<EnvironmentController> logger,
			IConfiguration configuration)
		{
			_client = client;
			_logger = logger;
			_numberOfFans = configuration.GetValue<int>("Environment:NumberOfFans", 3);
			_numberOfHeaters = configuration.GetValue<int>("Environment:NumberOfHeaters", 3);
		}

		public async Task SetFanState(int fanId, bool isOn)
		{
			if (fanId < 1 || fanId > _numberOfFans)
			{
				throw new ArgumentOutOfRangeException(nameof(fanId), $"Fan ID must be between 1 and {_numberOfFans}");
			}

			try
			{
				var response = await _client.PostAsync($"api/fans/{fanId}",
				new StringContent(isOn.ToString().ToLower(), System.Text.Encoding.UTF8, "application/json"));
				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Failed to set fan state for fan {FanId}: {Reason}", fanId, response.ReasonPhrase);
					Console.WriteLine($"Error: Failed to set fan state for fan {fanId}: {response.ReasonPhrase}");
					return;
				}
				else
				{
					Console.WriteLine($"Fan {fanId} has been turned {(isOn ? "On" : "Off")}.");
					_logger.LogInformation($"Fan {fanId} has been turned {(isOn ? "On" : "Off")}.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				_logger.LogError("Failed to set fan state for fan {FanId}", fanId);
			}
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

			try
			{
				var response = await _client.PostAsync($"api/heat/{heaterId}",
				new StringContent(level.ToString(), System.Text.Encoding.UTF8, "application/json"));
				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Failed to set heater level for heater {HeaterId}: {Reason}", heaterId, response.ReasonPhrase);
					Console.WriteLine($"Error: Failed to set heater level for heater {heaterId}: {response.ReasonPhrase}");
					return;
				}
				else
				{
					Console.WriteLine($"Heater {heaterId} has been set to level {level}.");
					_logger.LogInformation($"Heater {heaterId} has been set to level {level}.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				_logger.LogError("Failed to set heater level for heater {HeaterId}", heaterId);
			}
		}

		public async Task<bool> GetFanState(int id)
		{
			if (id < 1 || id > _numberOfFans)
			{
				throw new ArgumentOutOfRangeException(nameof(id), $"Fan ID must be between 1 and {_numberOfFans}");
			}

			try
			{
				var response = await _client.GetAsync($"api/fans/{id}/state");
				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Failed to get fan state for fan {FanId}: {Reason}", id, response.ReasonPhrase);
					Console.WriteLine($"Error: Failed to get fan state for fan {id}: {response.ReasonPhrase}");
					return false;
				}

				var content = await response.Content.ReadAsStringAsync();
				var fan = JsonSerializer.Deserialize<Fan>(content, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});
				_logger.LogInformation("Fan {FanId} state: {State}", id, fan.IsOn ? "On" : "Off");
				return fan.IsOn;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				_logger.LogError("Failed to get fan state for fan {FanId}", id);
				return false;
			}
		}

		public async Task<int> GetHeaterLevel(int id)
		{
			if (id < 1 || id > _numberOfHeaters)
			{
				throw new ArgumentOutOfRangeException(nameof(id), $"Heater ID must be between 1 and {_numberOfHeaters}");
			}

			try
			{
				var response = await _client.GetAsync($"api/heat/{id}/level");
				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Failed to get heater level for heater {HeaterId}: {Reason}", id, response.ReasonPhrase);
					Console.WriteLine($"Error: Failed to get heater level for heater {id}: {response.ReasonPhrase}");
					return 0;
				}

				var content = await response.Content.ReadAsStringAsync();
				var level = int.Parse(content);
				_logger.LogInformation("Heater {HeaterId} level: {Level}", id, level);
				return level;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				_logger.LogError("Failed to get heater level for heater {HeaterId}", id);
				return 0;
			}
		}

		public async Task<double> GetSensorTemperature(int sensorId)
		{
			try
			{
				var response = await _client.GetAsync($"api/sensor/{sensorId}");
				if (response.IsSuccessStatusCode)
				{
					var tempString = await response.Content.ReadAsStringAsync();
					var temperature = double.Parse(tempString);
					_logger.LogInformation("Sensor {SensorId} temperature: {Temperature}°C", sensorId, temperature);
					return temperature;
				}

				_logger.LogError("Failed to get temperature from sensor {SensorId}: {Reason}", sensorId, response.ReasonPhrase);
				Console.WriteLine($"Error: Failed to get temperature from sensor {sensorId}: {response.ReasonPhrase}");
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				_logger.LogError("Failed to get temperature from sensor {SensorId}", sensorId);
				return 0;
			}
		}

		public async Task<string> GetSensor1Temperature()
		{
			try
			{
				var response = await _client.GetAsync("api/Sensor/sensor1");
				if (response.IsSuccessStatusCode)
				{
					var temperature = await response.Content.ReadAsStringAsync();
					_logger.LogInformation("Sensor 1 temperature: {Temperature}°C", temperature);
					return temperature;
				}
				_logger.LogError("Failed to get temperature from Sensor 1: {Reason}", response.ReasonPhrase);
				Console.WriteLine($"Error: Failed to get temperature from Sensor 1: {response.ReasonPhrase}");
				return "0";
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				_logger.LogError("Failed to get temperature from Sensor 1");
				return "0";
			}
		}

		public async Task<int> GetSensor2Temperature()
		{
			try
			{
				var response = await _client.GetAsync("api/Sensor/sensor2");
				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStringAsync();
					if (int.TryParse(content, out int temp))
					{
						_logger.LogInformation("Sensor 2 temperature: {Temperature}°C", temp);
						return temp;
					}
					_logger.LogError("Failed to parse Sensor 2 temperature as an integer");
					Console.WriteLine("Error: Failed to parse Sensor 2 temperature as an integer");
					return 0;
				}
				_logger.LogError("Failed to get temperature from Sensor 2: {Reason}", response.ReasonPhrase);
				Console.WriteLine($"Error: Failed to get temperature from Sensor 2: {response.ReasonPhrase}");
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				_logger.LogError("Failed to get temperature from Sensor 2");
				return 0;
			}
		}

		public async Task<decimal> GetSensor3Temperature()
		{
			try
			{
				var response = await _client.GetAsync("api/Sensor/sensor3");
				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStringAsync();
					if (decimal.TryParse(content, out decimal temp))
					{
						_logger.LogInformation("Sensor 3 temperature: {Temperature}°C", temp);
						return temp;
					}
					_logger.LogError("Failed to parse Sensor 3 temperature as a decimal");
					Console.WriteLine("Error: Failed to parse Sensor 3 temperature as a decimal");
					return 0;
				}
				_logger.LogError("Failed to get temperature from Sensor 3: {Reason}", response.ReasonPhrase);
				Console.WriteLine($"Error: Failed to get temperature from Sensor 3: {response.ReasonPhrase}");
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				_logger.LogError("Failed to get temperature from Sensor 3");
				return 0;
			}
		}

		public async Task<double> GetAverageTemperature()
		{
			try
			{
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
				for (int i = 1; i <= _numberOfHeaters; i++)
				{
					await SetHeaterLevel(i, level);
				}
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
				for (int i = 1; i <= _numberOfFans; i++)
				{
					await SetFanState(i, state);
				}
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
				for (int i = 1; i <= _numberOfFans; i++)
				{
					var fanResponse = await GetFanState(i);
					Console.WriteLine($"  Fan {i}: {(fanResponse ? "On" : "Off")}");
					_logger.LogInformation("Fan {FanId}: {State}", i, fanResponse ? "On" : "Off");
				}

				Console.WriteLine("\nFetching heater levels individually...");
				_logger.LogInformation("Fetching heater levels individually...");
				for (int i = 1; i <= _numberOfHeaters; i++)
				{
					var heaterLevel = await GetHeaterLevel(i);
					Console.WriteLine($"  Heater {i}: Level {heaterLevel}");
					_logger.LogInformation("Heater {HeaterId}: Level {Level}", i, heaterLevel);
				}

				Console.WriteLine("\nFetching sensor temperatures individually...");
				_logger.LogInformation("Fetching sensor temperatures individually...");
				try
				{
					var sensor1Temp = await GetSensor1Temperature();
					Console.WriteLine($"  Sensor 1: Temperature {sensor1Temp}°C");
					_logger.LogInformation("Sensor 1: Temperature {Temperature}°C", sensor1Temp);

					var sensor2Temp = await GetSensor2Temperature();
					Console.WriteLine($"  Sensor 2: Temperature {sensor2Temp}°C");
					_logger.LogInformation("Sensor 2: Temperature {Temperature}°C", sensor2Temp);

					var sensor3Temp = await GetSensor3Temperature();
					Console.WriteLine($"  Sensor 3: Temperature {sensor3Temp}°C");
					_logger.LogInformation("Sensor 3: Temperature {Temperature}°C", sensor3Temp);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"  Error fetching sensor data: {ex.Message}");
					_logger.LogError("Error fetching sensor data");
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
				var response = await _client.PostAsync("api/Envo/reset", null);
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
