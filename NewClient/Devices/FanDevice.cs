using NewClient.Interfaces;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewClient.Models;

namespace NewClient.Devices
{
    public class FanDevice : IDevice
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;
        public int Id { get; }

        public FanDevice(HttpClient client, ILogger logger, int _id)
        {
            _client = client;
            _logger = logger;
            Id = _id;
        }

        public async Task<bool> GetState()
        {
            try
            {
                var response = await _client.GetAsync($"api/fans/{Id}/state");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get fan state for fan {FanId}: {Reason}", Id, response.ReasonPhrase);
                    Console.WriteLine($"Error: Failed to get fan state for fan {Id}: {response.ReasonPhrase}");
                    return false;
                }

                var content = await response.Content.ReadAsStringAsync();
                var fan = JsonSerializer.Deserialize<Fan>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                _logger.LogInformation("Fan {FanId} state: {State}", fan.Id, fan.IsOn ? "On" : "Off");
                return fan.IsOn;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                _logger.LogError("Failed to get fan state for fan {FanId}", Id);
                return false;
            }
        }

        public async Task SetState(bool state)
        {
            try
            {
                var response = await _client.PostAsync($"api/fans/{Id}",
                    new StringContent(state.ToString().ToLower(), System.Text.Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to set fan state for fan {FanId}: {Reason}", Id, response.ReasonPhrase);
                    Console.WriteLine($"Error: Failed to set fan state for fan {Id}: {response.ReasonPhrase}");
                    return;
                }
                else
                {
                    Console.WriteLine($"Fan {Id} has been turned {(state ? "On" : "Off")}.");
                    _logger.LogInformation($"Fan {Id} has been turned {(state ? "On" : "Off")}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                _logger.LogError("Failed to set fan state for fan {FanId}", Id);
            }
        }

        public Task<int> GetLevel() => Task.FromResult(0);
        public Task SetLevel(int level) => Task.CompletedTask;
        public Task<double> GetTemperature() => Task.FromResult(0.0);
    }
} 