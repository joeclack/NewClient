using System.Net.Http;

namespace NewClient.Interfaces
{
    public interface IHttpClientFactory
    {
        HttpClient CreateClient();
    }
} 