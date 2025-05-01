using NewClient.Interfaces;
using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using NewClient.Devices;

namespace NewClient.Factories
{
    // pretty simple factory - just creates the right device based on type
    // could probably add some validation or device-specific config here if needed
    public class DeviceFactory
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public DeviceFactory(HttpClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        // switch expression makes this nice and clean
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

    // might need to add more device types later if new hardware is added, but keeps it modular
    public enum DeviceType
    {
        Fan,
        Heater,
        Sensor
    }
} 