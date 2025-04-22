using NewClient.Models;
using System.Text.Json;

namespace NewClient.Controllers
{
	public class EnvironmentController
	{
		public readonly HttpClient _client;

		public EnvironmentController(HttpClient client)
		{
			_client = client;
		}

		public async Task SetFanState(int fanId, bool isOn)
		{
			var response = await _client.PostAsync($"api/fans/{fanId}",
			new StringContent(isOn.ToString().ToLower(), System.Text.Encoding.UTF8, "application/json"));
			if ( !response.IsSuccessStatusCode )
			{
				throw new Exception($"Failed to set fan state for fan {fanId}: {response.ReasonPhrase}");
			}
		}

		public async Task SetHeaterLevel(int heaterId, int level)
		{
			var response = await _client.PostAsync($"api/heat/{heaterId}",
			new StringContent(level.ToString(), System.Text.Encoding.UTF8, "application/json"));
			if ( !response.IsSuccessStatusCode )
			{
				throw new Exception($"Failed to set heater level {heaterId}: {response.ReasonPhrase}");
			}
		}

		public async Task<HttpResponseMessage> GetFanState(int id)
		{
			return await _client.GetAsync($"api/fans/{id}/state");
		}

		public async Task<HttpResponseMessage> GetHeaterLevel(int id)
		{
			return await _client.GetAsync($"api/heat/{id}/level");
		}

		public async Task<double> GetSensorTemperature(int sensorId)
		{
			var response = await _client.GetAsync($"api/sensor/{sensorId}");
			if ( response.IsSuccessStatusCode )
			{
				var tempString = await response.Content.ReadAsStringAsync();
				return double.Parse(tempString);
			}

			throw new Exception($"Failed to get temperature from sensor {sensorId}: {response.ReasonPhrase}");
		}

		public async Task<string> GetSensor1Temperature()
		{
			var response = await _client.GetAsync("api/Sensor/sensor1");
			if ( response.IsSuccessStatusCode )
			{
				return await response.Content.ReadAsStringAsync();
			}
			throw new Exception($"Failed to get temperature from Sensor 1: {response.ReasonPhrase}");
		}

		public async Task<int> GetSensor2Temperature()
		{
			var response = await _client.GetAsync("api/Sensor/sensor2");
			if ( response.IsSuccessStatusCode )
			{
				var content = await response.Content.ReadAsStringAsync();
				if ( int.TryParse(content, out int temp) )
				{
					return temp;
				}
				throw new Exception("Failed to parse Sensor 2 temperature as an integer.");
			}
			throw new Exception($"Failed to get temperature from Sensor 2: {response.ReasonPhrase}");
		}

		public async Task<decimal> GetSensor3Temperature()
		{
			var response = await _client.GetAsync("api/Sensor/sensor3");
			if ( response.IsSuccessStatusCode )
			{
				var content = await response.Content.ReadAsStringAsync();
				if ( decimal.TryParse(content, out decimal temp) )
				{
					return temp;
				}
				throw new Exception("Failed to parse Sensor 3 temperature as a decimal.");
			}
			throw new Exception($"Failed to get temperature from Sensor 3: {response.ReasonPhrase}");
		}

		public async Task<double> GetAverageTemperature()		
		{
			// Fetch sensor temperatures and calculate the average
			var sensor1 = double.Parse(await GetSensor1Temperature());
			var sensor2 = await GetSensor2Temperature();
			var sensor3 = (double)await GetSensor3Temperature();

			double avgTemperature = (sensor1 + sensor2 + sensor3) / 3;
			return avgTemperature;
		}

		public async Task SetAllHeaters(int level)
		{
			for ( int i = 1; i <= 3; i++ ) // Assuming 3 heaters
			{
				await SetHeaterLevel(i, level);
			}
		}

		public async Task SetAllFans(bool state)
		{
			for ( int i = 1; i <= 3; i++ ) // Assuming 3 fans
			{
				await SetFanState(i, state);
			}
		}

		public async Task GetAllStates()
		{
			try
			{
				Console.WriteLine("Fetching fan states individually...");
				for ( int i = 1; i <= 3; i++ ) // Assuming there are 3 fans for this example
				{
					var fanResponse = await GetFanState(i);
					if ( fanResponse.IsSuccessStatusCode )
					{
						var fanJson = await fanResponse.Content.ReadAsStringAsync();
						var fan = JsonSerializer.Deserialize<Fan>(fanJson, new JsonSerializerOptions
						{
							PropertyNameCaseInsensitive = true
						});
						Console.WriteLine($"  Fan {fan.Id}: {(fan.IsOn ? "On" : "Off")}");
					}
					else
					{
						Console.WriteLine($"  Fan {i}: Failed to fetch state.");
					}
				}
				Console.WriteLine("Fetching heater levels individually...");
				for ( int i = 1; i <= 3; i++ ) // Assuming there are 3 heaters for this example
				{
					var heaterResponse = await GetHeaterLevel(i);
					if ( heaterResponse.IsSuccessStatusCode )
					{
						var levelString = await heaterResponse.Content.ReadAsStringAsync();
						if ( int.TryParse(levelString, out int level) )
						{
							Console.WriteLine($"  Heater {i}: Level {level}");
						}
						else
						{
							Console.WriteLine($"  Heater {i}: Failed to parse level.");
						}
					}
					else
					{
						Console.WriteLine($"  Heater {i}: Failed to fetch level.");
					}
				}
				Console.WriteLine("Fetching sensor temperatures individually...");
				try
				{
					var sensor1Temp = await GetSensor1Temperature();
					Console.WriteLine($"  Sensor 1: Temperature {sensor1Temp} (Deg)");

					var sensor2Temp = await GetSensor2Temperature();
					Console.WriteLine($"  Sensor 2: Temperature {sensor2Temp} (Deg)");

					var sensor3Temp = await GetSensor3Temperature();
					Console.WriteLine($"  Sensor 3: Temperature {sensor3Temp} (Deg)");
				}
				catch ( Exception ex )
				{
					Console.WriteLine($"Error fetching sensor data: {ex.Message}");
				}
			}
			catch ( Exception ex )
			{
				Console.WriteLine($"Error fetching device states: {ex.Message}");
			}
		}

		public async Task<double> HoldTemperature(double currentTemperature, double targetTemperature, int durationSeconds)
		{
			Console.WriteLine($"Holding temperature at {targetTemperature}°C for {durationSeconds} seconds...");
			int intervalMs = 1000; // 1-second intervals

			for ( int i = 0; i < durationSeconds; i++ )
			{
				if ( currentTemperature < targetTemperature )
				{
					// Turn on heaters slightly and reduce fans
					await SetAllHeaters(1); // Minimal heating
					await SetAllFans(false); // Reduce cooling
				}
				else if ( currentTemperature > targetTemperature )
				{
					// Turn off heaters and increase fans
					await SetAllHeaters(0); // Turn off heating
					await SetAllFans(true); // Activate cooling
				}

				// Wait for a second and fetch the updated temperature
				await Task.Delay(intervalMs);
				currentTemperature = await GetAverageTemperature();
				Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");
			}

			return currentTemperature;
		}

		public async Task<double> AdjustTemperature(double currentTemperature, double targetTemperature, int durationSeconds)
		{
			Console.WriteLine($"Adjusting temperature to {targetTemperature}°C over {durationSeconds} seconds...");
			int intervalMs = 1000; // 1-second intervals
			int iterations = durationSeconds;

			for ( int i = 0; i < iterations; i++ )
			{
				if ( Math.Abs(currentTemperature - targetTemperature) <= 0.1 ) break;

				if ( currentTemperature < targetTemperature )
				{
					// Turn on heaters and reduce fan activity
					await SetAllHeaters(3); // Set heaters to level 3
					await SetAllFans(false); // Turn off fans
				}
				else
				{
					// Turn off heaters and increase fan activity
					await SetAllHeaters(0); // Turn off heaters
					await SetAllFans(true); // Turn on fans
				}

				// Wait for a second and fetch the updated temperature
				await Task.Delay(intervalMs);
				currentTemperature = await GetAverageTemperature();
				Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");
			}

			return currentTemperature;
		}

		public async Task<HttpResponseMessage> ResetEnvironment()
		{
			return await _client.PostAsync("api/Envo/reset", null);
		}
	}
}
