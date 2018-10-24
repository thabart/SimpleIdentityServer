using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Builders;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Selectors;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.UserInfoIntrospection;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests.Introspections
{
    public class UserInfoIntrospectionHandlerFixture : IClassFixture<TestOauthServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IClientAuthSelector _clientAuthSelector;

        public UserInfoIntrospectionHandlerFixture(TestOauthServerFixture server)
        {
            _server = server;
        }
                
        [Fact]
        public async Task When_Introspect_Identity_Token_Then_Claims_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UsePassword("superuser", "password", "role")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");
            var authResult = await UserInfoIntrospectionHandler.HandleAuthenticate(_httpClientFactoryStub.Object, baseUrl + "/.well-known/openid-configuration", result.Content.AccessToken);

            // ASSERT
            Assert.True(authResult.Succeeded);
        }

        private void InitializeFakeObjects()
        {
            var services = new ServiceCollection();
            services.AddSimpleIdentityServerJwt();
            var provider = services.BuildServiceProvider();
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var requestBuilder = new RequestBuilder();
            var postTokenOperation = new PostTokenOperation(_httpClientFactoryStub.Object);
            var getDiscoveryOperation = new GetDiscoveryOperation(_httpClientFactoryStub.Object);
            var introspectionOperation = new IntrospectOperation(_httpClientFactoryStub.Object);
            var revokeTokenOperation = new RevokeTokenOperation(_httpClientFactoryStub.Object);
            var getJsonWebKeysOperation = new GetJsonWebKeysOperation(_httpClientFactoryStub.Object);
            _clientAuthSelector = new ClientAuthSelector(
                new TokenClientFactory(postTokenOperation, getDiscoveryOperation),
                new IntrospectClientFactory(introspectionOperation, getDiscoveryOperation),
                new RevokeTokenClientFactory(revokeTokenOperation, getDiscoveryOperation));
        }
    }
}