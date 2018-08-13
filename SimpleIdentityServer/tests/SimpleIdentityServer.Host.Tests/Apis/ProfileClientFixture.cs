using Moq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Selectors;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Core.Common.DTOs.Requests;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests.Apis
{
    public class ProfileClientFixture : IClassFixture<TestOauthServerFixture>
    {
        const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IProfileClient _profileClient;
        private IClientAuthSelector _clientAuthSelector;

        public ProfileClientFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        #region Errors

        #region Link profile

        [Fact]
        public async Task When_Link_Profile_And_No_UserId_Is_Passed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_profile")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            // ACT
            var result = await _profileClient.LinkProfile(baseUrl + "/profiles", "currentSubject", new LinkProfileRequest
            {

            }, grantedToken.Content.AccessToken);

            // ASSERT
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter user_id is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Link_Profile_And_No_Issuer_Is_Passed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_profile")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            // ACT
            var result = await _profileClient.LinkProfile(baseUrl + "/profiles", "currentSubject", new LinkProfileRequest
            {
                UserId = "user_id"
            }, grantedToken.Content.AccessToken);

            // ASSERT
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter issuer is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Link_Profile_And_User_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_profile")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            // ACT
            var result = await _profileClient.LinkProfile(baseUrl + "/profiles", "currentSubject", new LinkProfileRequest
            {
                UserId = "user_id",
                Issuer = "issuer"
            }, grantedToken.Content.AccessToken);

            // ASSERT
            Assert.True(result.ContainsError);
            Assert.Equal("internal_error", result.Error.Error);
            Assert.Equal("the resource owner doesn't exist", result.Error.ErrorDescription);
        }

        #endregion

        #region Unlink profile

        [Fact]
        public async Task When_Unlink_Profile_And_User_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_profile")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            // ACT
            var result = await _profileClient.UnlinkProfile(baseUrl + "/profiles", "externalSubject", "currentSubject", grantedToken.Content.AccessToken);

            // ASSERT
            Assert.True(result.ContainsError);
            Assert.Equal("internal_error", result.Error.Error);
            Assert.Equal("the resource owner doesn't exist", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Unlink_Profile_And_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_profile")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            // ACT
            var result = await _profileClient.UnlinkProfile(baseUrl + "/profiles", "invalid_external_subject", "administrator", grantedToken.Content.AccessToken);

            // ASSERT
            Assert.True(result.ContainsError);
            Assert.Equal("internal_error", result.Error.Error);
            Assert.Equal("not authorized to remove the profile", result.Error.ErrorDescription);
        }

        #endregion

        #region Get profiles

        [Fact]
        public async Task When_Get_Profiles_And_User_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_profile")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            // ACT
            var result = await _profileClient.GetProfiles(baseUrl + "/profiles", "notvalid", grantedToken.Content.AccessToken);

            // ASSERT
            Assert.True(result.ContainsError);
            Assert.Equal("internal_error", result.Error.Error);
            Assert.Equal("the resource owner doesn't exist", result.Error.ErrorDescription);
        }


        #endregion

        #endregion

        #region Happy paths

        #region Link profile

        [Fact]
        public async Task When_Link_Profile_Then_Ok_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_profile")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            // ACT
            var result = await _profileClient.LinkProfile(baseUrl + "/profiles", "administrator", new LinkProfileRequest
            {
                UserId = "user_id_1",
                Issuer = "issuer"
            }, grantedToken.Content.AccessToken);

            // ASSERT
            Assert.False(result.ContainsError);
        }

        #endregion

        #region Unlink profile

        [Fact]
        public async Task When_Unlink_Profile_Then_Ok_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_profile")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var linkResult = await _profileClient.LinkProfile(baseUrl + "/profiles", "administrator", new LinkProfileRequest
            {
                UserId = "user_id",
                Issuer = "issuer"
            }, grantedToken.Content.AccessToken);

            // ACT
            var unlinkResult = await _profileClient.UnlinkProfile(baseUrl + "/profiles", "user_id", "administrator", grantedToken.Content.AccessToken);

            // ASSERT
            Assert.False(unlinkResult.ContainsError);
        }

        #endregion

        #region Get profiles

        [Fact]
        public async Task When_Get_Profiles_Then_List_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_profile")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var linkResult = await _profileClient.LinkProfile(baseUrl + "/profiles", "administrator", new LinkProfileRequest
            {
                UserId = "user_id",
                Issuer = "issuer"
            }, grantedToken.Content.AccessToken);

            // ACT
            var getProfilesResult = await _profileClient.GetProfiles(baseUrl + "/profiles", "administrator", grantedToken.Content.AccessToken);

            // ASSERT
            Assert.False(getProfilesResult.ContainsError);
            Assert.True(getProfilesResult.Content.Count() >= 1);

        }

        #endregion

        #endregion

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var linkProfileOperation = new LinkProfileOperation(_httpClientFactoryStub.Object);
            var unlinkProfileOperation = new UnlinkProfileOperation(_httpClientFactoryStub.Object);
            var getProfilesOperation = new GetProfilesOperation(_httpClientFactoryStub.Object);
            _profileClient = new ProfileClient(linkProfileOperation, unlinkProfileOperation, getProfilesOperation);
            var postTokenOperation = new PostTokenOperation(_httpClientFactoryStub.Object);
            var getDiscoveryOperation = new GetDiscoveryOperation(_httpClientFactoryStub.Object);
            var introspectionOperation = new IntrospectOperation(_httpClientFactoryStub.Object);
            var revokeTokenOperation = new RevokeTokenOperation(_httpClientFactoryStub.Object);
            _clientAuthSelector = new ClientAuthSelector(
                new TokenClientFactory(postTokenOperation, getDiscoveryOperation),
                new IntrospectClientFactory(introspectionOperation, getDiscoveryOperation),
                new RevokeTokenClientFactory(revokeTokenOperation, getDiscoveryOperation));
        }
    }
}
