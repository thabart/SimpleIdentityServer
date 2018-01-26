using System.Net.Http;

namespace SimpleIdentityServer.Manager.Client.Factories
{
    public interface IHttpClientFactory
    {
        HttpClient GetHttpClient();
    }

    internal sealed class HttpClientFactory : IHttpClientFactory
    {
        public HttpClient GetHttpClient()
        {
            return new HttpClient();
        }
    }
}
