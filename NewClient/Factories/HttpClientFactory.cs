using System.Net.Http;
using Microsoft.Extensions.Configuration;
using NewClient.Interfaces;
using IHttpClientFactory = NewClient.Interfaces.IHttpClientFactory;

namespace NewClient.Factories
{
    public class HttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public HttpClientFactory(IConfiguration configuration)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetValue<string>("Environment:BaseAddress"))
            };
            _client.DefaultRequestHeaders.Add("X-Api-Key", configuration.GetValue<string>("Environment:APIKey"));
        }

        public HttpClient CreateClient()
        {
            return _client;
        }
    }
} 