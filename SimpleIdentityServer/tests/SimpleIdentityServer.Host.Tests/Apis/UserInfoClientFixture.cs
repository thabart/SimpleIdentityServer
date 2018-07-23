using Moq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Selectors;
using SimpleIdentityServer.Common.Client.Factories;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests.Apis
{
    public class UserInfoClientFixture : IClassFixture<TestOauthServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IClientAuthSelector _clientAuthSelector;
        private IUserInfoClient _userInfoClient;

        public UserInfoClientFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Pass_Invalid_Token_To_UserInfo_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var getUserInfoResult = await _userInfoClient.Resolve(baseUrl + "/.well-known/openid-configuration", "invalid_access_token");

            // ASSERTS
            Assert.NotNull(getUserInfoResult);
            Assert.True(getUserInfoResult.ContainsError);
            Assert.Equal("invalid_token", getUserInfoResult.Error.Error);
            Assert.Equal("the token is not valid", getUserInfoResult.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Client_Access_Token_To_UserInfo_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("openid")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");
            var getUserInfoResult = await _userInfoClient.Resolve(baseUrl + "/.well-known/openid-configuration", result.Content.AccessToken);

            // ASSERTS
            Assert.NotNull(getUserInfoResult);
            Assert.True(getUserInfoResult.ContainsError);
            Assert.Equal("invalid_token", getUserInfoResult.Error.Error);
            Assert.Equal("not a valid resource owner token", getUserInfoResult.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Access_Token_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UsePassword("administrator", "password", "scim")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");
            var getUserInfoResult = await _userInfoClient.Resolve(baseUrl + "/.well-known/openid-configuration", result.Content.AccessToken);

            // ASSERTS
            Assert.NotNull(getUserInfoResult);
        }

        [Fact]
        public async Task When_Pass_Access_Token_Then_Jws_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client_userinfo_sig_rs256", "client_userinfo_sig_rs256")
                .UsePassword("administrator", "password", "scim")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");
            var getUserInfoResult = await _userInfoClient.Resolve(baseUrl + "/.well-known/openid-configuration", result.Content.AccessToken);

            // ASSERTS
            Assert.NotNull(getUserInfoResult);
            Assert.NotNull(getUserInfoResult.JwtToken);
        }

        [Fact]
        public async Task When_Pass_Access_Token_Then_Jwe_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client_userinfo_enc_rsa15", "client_userinfo_enc_rsa15")
                .UsePassword("administrator", "password", "scim")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");
            var getUserInfoResult = await _userInfoClient.Resolve(baseUrl + "/.well-known/openid-configuration", result.Content.AccessToken);

            // ASSERTS
            Assert.NotNull(getUserInfoResult);
            Assert.NotNull(getUserInfoResult.JwtToken);
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var postTokenOperation = new PostTokenOperation(_httpClientFactoryStub.Object);
            var getDiscoveryOperation = new GetDiscoveryOperation(_httpClientFactoryStub.Object);
            var introspectionOperation = new IntrospectOperation(_httpClientFactoryStub.Object);
            var revokeTokenOperation = new RevokeTokenOperation(_httpClientFactoryStub.Object);
            _clientAuthSelector = new ClientAuthSelector(
                new TokenClientFactory(postTokenOperation, getDiscoveryOperation),
                new IntrospectClientFactory(introspectionOperation, getDiscoveryOperation),
                new RevokeTokenClientFactory(revokeTokenOperation, getDiscoveryOperation));
            var getUserInfoOperation = new GetUserInfoOperation(_httpClientFactoryStub.Object);
            _userInfoClient = new UserInfoClient(getUserInfoOperation, getDiscoveryOperation);
        }
    }
}
