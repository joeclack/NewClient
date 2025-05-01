using NewClient.Interfaces;
using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using NewClient.Devices;
using IHttpClientFactory = NewClient.Interfaces.IHttpClientFactory;

namespace NewClient.Factories
{
    // pretty simple factory - just creates the right device based on type
    // could probably add some validation or device-specific config here if needed
    public class DeviceFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public DeviceFactory(IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // switch expression makes this nice and clean
        public IDevice CreateDevice(DeviceType type, int id)
        {
            var client = _httpClientFactory.CreateClient();
            return type switch
            {
                DeviceType.Fan => new FanDevice(client, _logger, id),
                DeviceType.Heater => new HeaterDevice(client, _logger, id),
                DeviceType.Sensor => new SensorDevice(client, _logger, id),
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