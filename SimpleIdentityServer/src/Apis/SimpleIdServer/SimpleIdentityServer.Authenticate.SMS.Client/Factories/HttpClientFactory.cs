using System.Net.Http;

namespace SimpleIdentityServer.Authenticate.SMS.Client.Factories
{
    public interface IHttpClientFactory
    {
        HttpClient GetHttpClient();
    }

    internal class HttpClientFactory : IHttpClientFactory
    {
        public HttpClient GetHttpClient()
        {
            var httpHandler = new HttpClientHandler();
#if NET
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
#else
            httpHandler.ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true;
#endif
            return new HttpClient(httpHandler);
        }
    }
}
