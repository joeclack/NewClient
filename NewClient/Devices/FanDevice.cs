using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewClient.Interfaces;
using NewClient.Models;

namespace NewClient.Devices
{
    public class FanDevice : IDevice
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;
        public int Id { get; }

        public FanDevice(HttpClient client, ILogger logger, int id)
        {
            _client = client;
            _logger = logger;
            Id = id;
        }

        public async Task<DeviceStateResult> GetState()
        {
            try
            {
                var response = await _client.GetAsync($"api/fans/{Id}/state");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get fan state for fan {FanId}: {Reason}", Id, response.ReasonPhrase);
                    return new DeviceStateResult { HasError = true, ErrorMessage = response.ReasonPhrase };
                }

                var content = await response.Content.ReadAsStringAsync();
                var fan = JsonSerializer.Deserialize<Fan>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                _logger.LogInformation("Fan {FanId} state: {State}", fan.Id, fan.IsOn ? "On" : "Off");
                return new DeviceStateResult { IsOn = fan.IsOn, HasError = false };
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get fan state for fan {FanId}: {Error}", Id, ex.Message);
                return new DeviceStateResult { HasError = true, ErrorMessage = ex.Message };
            }
        }

        public async Task SetState(bool isOn)
        {
            try
            {
                var response = await _client.GetAsync($"api/fans/{Id}/state");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to set fan state for fan {FanId}: {Reason}", Id, response.ReasonPhrase);
                    return;
                }
                _logger.LogInformation("Fan {FanId} has been turned {State}", Id, isOn ? "On" : "Off");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to set fan state for fan {FanId}: {Error}", Id, ex.Message);
            }
        }

        public Task<int> GetLevel() => Task.FromResult(0);
        public Task SetLevel(int level) => Task.CompletedTask;
        public Task<double> GetTemperature() => Task.FromResult(0.0);
    }
} 