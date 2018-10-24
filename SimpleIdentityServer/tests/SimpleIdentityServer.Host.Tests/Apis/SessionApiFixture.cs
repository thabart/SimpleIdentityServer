using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests.Apis
{
    public class SessionApiFixture : IClassFixture<TestOauthServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;

        public SessionApiFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Check_Session_Then_Ok_Is_Returned()
        {
            // ARRANGE
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new System.Uri($"{baseUrl}/check_session")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var html = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            // ASSERT
            Assert.Equal(HttpStatusCode.OK, httpResult.StatusCode);
        }

        [Fact]
        public async Task When_End_Session_Then_Ok_Is_Returned()
        {
            // ARRANGE
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new System.Uri($"{baseUrl}/end_session")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var html = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            // ASSERT
            Assert.Equal(HttpStatusCode.OK, httpResult.StatusCode);
        }
    }
}
