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

        public async Task<DeviceStateResult> GetState()
        {
            try
            {
                var level = await GetLevel();
                return new DeviceStateResult { IsOn = level > 0, HasError = false };
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get heater state for heater {HeaterId}: {Error}", Id, ex.Message);
                return new DeviceStateResult { HasError = true, ErrorMessage = ex.Message };
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
                _logger.LogError("Failed to set heater state for heater {HeaterId}: {Error}", Id, ex.Message);
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
                    return 0;
                }

                var content = await response.Content.ReadAsStringAsync();
                var level = int.Parse(content);
                _logger.LogInformation("Heater {HeaterId} level: {Level}", Id, level);
                return level;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get heater level for heater {HeaterId}: {Error}", Id, ex.Message);
                return 0;
            }
        }

        public async Task SetLevel(int level)
        {
            if (level < 0 || level > MaxLevel)
            {
                _logger.LogError("Heater level must be between 0 and {MaxLevel}", MaxLevel);
                return;
            }

            try
            {
                var response = await _client.PostAsync($"api/heat/{Id}",
                    new StringContent(level.ToString(), System.Text.Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to set heater level for heater {HeaterId}: {Reason}", Id, response.ReasonPhrase);
                    return;
                }
                _logger.LogInformation("Heater {HeaterId} has been set to level {Level}", Id, level);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to set heater level for heater {HeaterId}: {Error}", Id, ex.Message);
            }
        }

        public Task<double> GetTemperature() => Task.FromResult(0.0);
    }
} 