using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace SimpleIdentityServer.Module.Loader.Factories
{
    internal interface IHttpClientFactory
    {
        HttpClient GetHttpClient();
    }

    internal sealed class HttpClientFactory : IHttpClientFactory
    {
        public HttpClient GetHttpClient()
        {
            var httpHandler = new HttpClientHandler();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            }
            else
            {
                httpHandler.ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true;
            }

            return new HttpClient(httpHandler);
        }
    }
}
