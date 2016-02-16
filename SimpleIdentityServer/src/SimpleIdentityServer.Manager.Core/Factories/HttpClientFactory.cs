using System.Net.Http;

namespace SimpleIdentityServer.Manager.Core.Factories
{
    public interface IHttpClientFactory
    {
        HttpClient GetHttpClient();
    }

    public class HttpClientFactory : IHttpClientFactory
    {
        public HttpClient GetHttpClient()
        {
            return new HttpClient();
        }
    }
}
