using NewClient.Interfaces;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NewClient.Devices
{
    public class HeaterDevice : IDevice
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;
        public int Id { get; }
        private const int MaxLevel = 3;

        public HeaterDevice(HttpClient client, ILogger logger, int id)
        {
            _client = client;
            _logger = logger;
            Id = id;
        }

        public async Task<bool> GetState()
        {
            try
            {
                var level = await GetLevel();
                return level > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                _logger.LogError("Failed to get heater state for heater {HeaterId}", Id);
                return false;
            }
        }

        public async Task SetState(bool state)
        {
            try
            {
                await SetLevel(state ? MaxLevel : 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                _logger.LogError("Failed to set heater state for heater {HeaterId}", Id);
            }
        }

        public async Task<int> GetLevel()
        {
            try
            {
                var response = await _client.GetAsync($"api/heat/{Id}/level");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get heater level for heater {HeaterId}: {Reason}", Id, response.ReasonPhrase);
                    Console.WriteLine($"Error: Failed to get heater level for heater {Id}: {response.ReasonPhrase}");
                    return 0;
                }

                var content = await response.Content.ReadAsStringAsync();
                var level = int.Parse(content);
                _logger.LogInformation("Heater {HeaterId} level: {Level}", Id, level);
                return level;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                _logger.LogError("Failed to get heater level for heater {HeaterId}", Id);
                return 0;
            }
        }

        public async Task SetLevel(int level)
        {
            if (level < 0 || level > MaxLevel)
            {
                Console.WriteLine($"Error: Heater level must be between 0 and {MaxLevel}");
                return;
            }

            try
            {
                var response = await _client.PostAsync($"api/heat/{Id}",
                    new StringContent(level.ToString(), System.Text.Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to set heater level for heater {HeaterId}: {Reason}", Id, response.ReasonPhrase);
                    Console.WriteLine($"Error: Failed to set heater level for heater {Id}: {response.ReasonPhrase}");
                    return;
                }
                else
                {
                    Console.WriteLine($"Heater {Id} has been set to level {level}.");
                    _logger.LogInformation($"Heater {Id} has been set to level {level}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                _logger.LogError("Failed to set heater level for heater {HeaterId}", Id);
            }
        }

        public Task<double> GetTemperature() => Task.FromResult(0.0);
    }
} 