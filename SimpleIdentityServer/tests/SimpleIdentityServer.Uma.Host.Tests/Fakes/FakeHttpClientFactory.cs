using Microsoft.AspNetCore.TestHost;
using SimpleIdentityServer.Common.Client.Factories;
using System.Net.Http;

namespace SimpleIdentityServer.Uma.Host.Tests.Fakes
{
    public class FakeHttpClientFactory : IHttpClientFactory
    {
        private static FakeHttpClientFactory _instance;
        private TestServer _server;

        private FakeHttpClientFactory()
        {
        }

        public static FakeHttpClientFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FakeHttpClientFactory();
                }

                return _instance;
            }
        }


        public void Set(TestServer server)
        {
            _server = server;
        }

        public HttpClient GetHttpClient()
        {
            return _server.CreateClient();
        }
    }
}
