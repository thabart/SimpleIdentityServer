using System.Net.Http;

namespace SimpleIdentityServer.Common.Client.Factories
{
    public interface IHttpClientFactory
    {
        HttpClient GetHttpClient();
    }

    public class HttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _httpClient;

        public HttpClientFactory()
        {
            _httpClient = new HttpClient();
        }

        public HttpClient GetHttpClient()
        {
            return _httpClient;
        }
    }
}