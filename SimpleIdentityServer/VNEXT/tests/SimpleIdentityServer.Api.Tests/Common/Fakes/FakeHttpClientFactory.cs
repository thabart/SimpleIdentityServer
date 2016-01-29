using System.Net.Http;
using SimpleIdentityServer.Core.Factories;

namespace SimpleIdentityServer.Api.Tests.Common.Fakes
{
    public class FakeHttpClientFactory : IHttpClientFactory
    {
        public HttpClient HttpClient { private get; set; }


        public HttpClient GetHttpClient()
        {
            return HttpClient;
        }
    }
}
