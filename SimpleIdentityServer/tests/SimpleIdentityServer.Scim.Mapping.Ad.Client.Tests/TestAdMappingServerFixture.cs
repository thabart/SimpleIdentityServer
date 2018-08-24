using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Net.Http;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Tests
{
    public class TestAdMappingServerFixture : IDisposable
    {
        public TestServer Server { get; }
        public HttpClient Client { get; }

        public TestAdMappingServerFixture()
        {
            Server = new TestServer(new WebHostBuilder().UseUrls("http://localhost:5000").UseStartup<FakeStartup>());
            Client = Server.CreateClient();
        }

        public void Dispose()
        {
            Server.Dispose();
            Client.Dispose();
        }
    }
}
