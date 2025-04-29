using NewClient.Interfaces;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NewClient.Devices
{
    public class SensorDevice : IDevice
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;
        public int Id { get; }

        public SensorDevice(HttpClient client, ILogger logger, int id)
        {
            _client = client;
            _logger = logger;
            Id = id;
        }

        public async Task<bool> GetState()
        {
            try
            {
                var temp = await GetTemperature();
                return temp > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                _logger.LogError("Failed to get sensor state for sensor {SensorId}", Id);
                return false;
            }
        }

        public Task SetState(bool state) => Task.CompletedTask;
        public Task<int> GetLevel() => Task.FromResult(0);
        public Task SetLevel(int level) => Task.CompletedTask;

        public async Task<double> GetTemperature()
        {
            try
            {
                var endpoint = Id switch
                {
                    1 => "api/Sensor/sensor1",
                    2 => "api/Sensor/sensor2",
                    3 => "api/Sensor/sensor3",
                    _ => $"api/sensor/{Id}"
                };

                var response = await _client.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    var tempString = await response.Content.ReadAsStringAsync();
                    var temperature = double.Parse(tempString);
                    Console.WriteLine($"Sensor {Id} temperature: {temperature}°C");
                    _logger.LogInformation("Sensor {SensorId} temperature: {Temperature}°C", Id, temperature);
                    return temperature;
                }

                _logger.LogError("Failed to get temperature from sensor {SensorId}: {Reason}", Id, response.ReasonPhrase);
                Console.WriteLine($"Error: Failed to get temperature from sensor {Id}: {response.ReasonPhrase}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                _logger.LogError("Failed to get temperature from sensor {SensorId}", Id);
                return 0;
            }
        }
    }
} 