using NewClient.Interfaces;
using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using NewClient.Devices;

namespace NewClient.Factories
{
    public class DeviceFactory
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public DeviceFactory(HttpClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        public IDevice CreateDevice(DeviceType type, int id)
        {
            return type switch
            {
                DeviceType.Fan => new FanDevice(_client, _logger, id),
                DeviceType.Heater => new HeaterDevice(_client, _logger, id),
                DeviceType.Sensor => new SensorDevice(_client, _logger, id),
                _ => throw new ArgumentException($"Unknown device type: {type}")
            };
        }
    }

    public enum DeviceType
    {
        Fan,
        Heater,
        Sensor
    }
} 