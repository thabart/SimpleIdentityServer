using System;
using System.Net.Http.Headers;
using Moq;
using SimpleIdentityServer.Core.Api.Token;
using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using Xunit;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using SimpleIdServer.Bus;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.OAuth.Logging;

namespace SimpleIdentityServer.Core.UnitTests.Api.Token
{
    public sealed class TokenActionsFixture
    {
        private Mock<IGetTokenByResourceOwnerCredentialsGrantTypeAction>
            _getTokenByResourceOwnerCredentialsGrantTypeActionFake;
        private Mock<IGetTokenByAuthorizationCodeGrantTypeAction> _getTokenByAuthorizationCodeGrantTypeActionFake;
        private Mock<IResourceOwnerGrantTypeParameterValidator> _resourceOwnerGrantTypeParameterValidatorFake;
        private Mock<IAuthorizationCodeGrantTypeParameterTokenEdpValidator>
            _authorizationCodeGrantTypeParameterTokenEdptValidatorFake;
        private Mock<IOAuthEventSource> _oauthEventSource;
        private Mock<IGetTokenByRefreshTokenGrantTypeAction> _getTokenByRefreshTokenGrantTypeActionFake;
        private Mock<IRefreshTokenGrantTypeParameterValidator> _refreshTokenGrantTypeParameterValidatorFake;
        private Mock<IClientCredentialsGrantTypeParameterValidator> _clientCredentialsGrantTypeParameterValidatorStub;
        private Mock<IRevokeTokenParameterValidator> _revokeTokenParameterValidator;
        private Mock<IGetTokenByClientCredentialsGrantTypeAction> _getTokenByClientCredentialsGrantTypeActionStub;
        private Mock<IRevokeTokenAction> _revokeTokenActionStub;
        private Mock<IPayloadSerializer> _payloadSerializerStub;
        private ITokenActions _tokenActions;

        [Fact]
        public async Task When_Passing_No_Request_To_ResourceOwner_Grant_Type_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(null, null, null, null));
        }

        [Fact]
        public async Task When_Requesting_Token_Via_Resource_Owner_Credentials_Grant_Type_Then_Events_Are_Logged()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string userName = "userName";
            const string password = "password";
            const string accessToken = "accessToken";
            const string identityToken = "identityToken";
            var parameter = new ResourceOwnerGrantTypeParameter
            {
                ClientId = clientId,
                UserName = userName,
                Password = password
            };
            var grantedToken = new GrantedToken
            {
                AccessToken = accessToken,
                IdToken = identityToken
            };
            _getTokenByResourceOwnerCredentialsGrantTypeActionFake.Setup(
                g => g.Execute(It.IsAny<ResourceOwnerGrantTypeParameter>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<X509Certificate2>(), null))
                .Returns(Task.FromResult(grantedToken));

            // ACT
            await _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(parameter, null, null, null);

            // ASSERTS
            _oauthEventSource.Verify(s => s.StartGetTokenByResourceOwnerCredentials(clientId, userName, password));
            _oauthEventSource.Verify(s => s.EndGetTokenByResourceOwnerCredentials(accessToken, identityToken));
        }

        [Fact]
        public async Task When_Passing_No_Request_To_AuthorizationCode_Grant_Type_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.GetTokenByAuthorizationCodeGrantType(null, null, null, null));
        }

        [Fact]
        public async Task When_Requesting_Token_Via_AuthorizationCode_Grant_Type_Then_Events_Are_Logged()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string code = "code";
            const string accessToken = "accessToken";
            const string identityToken = "identityToken";
            var parameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientId = clientId,
                Code = code
            };
            var grantedToken = new GrantedToken
            {
                AccessToken = accessToken,
                IdToken = identityToken
            };
            _getTokenByAuthorizationCodeGrantTypeActionFake.Setup(
                g => g.Execute(It.IsAny<AuthorizationCodeGrantTypeParameter>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<X509Certificate2>(), null))
                .Returns(Task.FromResult(grantedToken));

            // ACT
            await _tokenActions.GetTokenByAuthorizationCodeGrantType(parameter, null, null, null);

            // ASSERTS
            _oauthEventSource.Verify(s => s.StartGetTokenByAuthorizationCode(clientId, code));
            _oauthEventSource.Verify(s => s.EndGetTokenByAuthorizationCode(accessToken, identityToken));
        }

        [Fact]
        public async Task When_Passing_No_Request_To_Refresh_Token_Grant_Type_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.GetTokenByRefreshTokenGrantType(null, null, null, null));
        }

        [Fact]
        public async Task When_Passing_Request_To_Refresh_Token_Grant_Type_Then_Events_Are_Logged()
        {            
            // ARRANGE
            InitializeFakeObjects();
            const string refreshToken = "refresh_token";
            const string accessToken = "accessToken";
            const string identityToken = "identityToken";
            var parameter = new RefreshTokenGrantTypeParameter
            {
                RefreshToken = refreshToken
            };
            var grantedToken = new GrantedToken
            {
                AccessToken = accessToken,
                IdToken = identityToken
            };
            _getTokenByRefreshTokenGrantTypeActionFake.Setup(
                g => g.Execute(It.IsAny<RefreshTokenGrantTypeParameter>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<X509Certificate2>(), null))
                .Returns(Task.FromResult(grantedToken));

            // ACT
            await _tokenActions.GetTokenByRefreshTokenGrantType(parameter, null, null, null);

            // ASSERTS
            _oauthEventSource.Verify(s => s.StartGetTokenByRefreshToken(refreshToken));
            _oauthEventSource.Verify(s => s.EndGetTokenByRefreshToken(accessToken, identityToken));
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_To_ClientCredentials_GrantType_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.GetTokenByClientCredentialsGrantType(null, null, null, null));
        }

        [Fact]
        public async Task When_Getting_Token_Via_ClientCredentials_GrantType_Then_GrantedToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string scope = "valid_scope";
            const string clientId = "valid_client_id";
            var parameter = new ClientCredentialsGrantTypeParameter
            {
                Scope = scope
            };
            var grantedToken = new GrantedToken
            {
                ClientId = clientId
            };
            _getTokenByClientCredentialsGrantTypeActionStub.Setup(g => g.Execute(It.IsAny<ClientCredentialsGrantTypeParameter>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<X509Certificate2>(), null))
                .Returns(Task.FromResult(grantedToken));

            // ACT
            var result = await _tokenActions.GetTokenByClientCredentialsGrantType(parameter, null, null, null);

            // ASSERTS
            _oauthEventSource.Verify(s => s.StartGetTokenByClientCredentials(scope));
            _oauthEventSource.Verify(s => s.EndGetTokenByClientCredentials(clientId, scope));
            Assert.NotNull(result);
            Assert.True(result.ClientId == clientId);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.RevokeToken(null, null, null, null));
        }

        [Fact]
        public void When_Revoking_Token_Then_Action_Is_Executed()
        {
            // ARRANGE
            const string accessToken = "access_token";
            InitializeFakeObjects();

            // ACT
            _tokenActions.RevokeToken(new RevokeTokenParameter
            {
                Token = accessToken
            }, null, null, null);

            // ASSERTS
            _oauthEventSource.Verify(s => s.StartRevokeToken(accessToken));
            _oauthEventSource.Verify(s => s.EndRevokeToken(accessToken));
        }

        private void InitializeFakeObjects()
        {
            _getTokenByResourceOwnerCredentialsGrantTypeActionFake = new Mock<IGetTokenByResourceOwnerCredentialsGrantTypeAction>();
            _getTokenByAuthorizationCodeGrantTypeActionFake = new Mock<IGetTokenByAuthorizationCodeGrantTypeAction>();
            _resourceOwnerGrantTypeParameterValidatorFake = new Mock<IResourceOwnerGrantTypeParameterValidator>();
            _authorizationCodeGrantTypeParameterTokenEdptValidatorFake = new Mock<IAuthorizationCodeGrantTypeParameterTokenEdpValidator>();
            _oauthEventSource = new Mock<IOAuthEventSource>();
            _getTokenByRefreshTokenGrantTypeActionFake = new Mock<IGetTokenByRefreshTokenGrantTypeAction>();
            _refreshTokenGrantTypeParameterValidatorFake = new Mock<IRefreshTokenGrantTypeParameterValidator>();
            _clientCredentialsGrantTypeParameterValidatorStub = new Mock<IClientCredentialsGrantTypeParameterValidator>();
            _getTokenByClientCredentialsGrantTypeActionStub = new Mock<IGetTokenByClientCredentialsGrantTypeAction>();
            _revokeTokenParameterValidator = new Mock<IRevokeTokenParameterValidator>();
            var eventPublisher = new Mock<IEventPublisher>();
            _payloadSerializerStub = new Mock<IPayloadSerializer>();
            _revokeTokenActionStub = new Mock<IRevokeTokenAction>();
            _tokenActions = new TokenActions(_getTokenByResourceOwnerCredentialsGrantTypeActionFake.Object,
                _getTokenByAuthorizationCodeGrantTypeActionFake.Object,
                _resourceOwnerGrantTypeParameterValidatorFake.Object,
                _authorizationCodeGrantTypeParameterTokenEdptValidatorFake.Object,
                _refreshTokenGrantTypeParameterValidatorFake.Object,
                _getTokenByRefreshTokenGrantTypeActionFake.Object,
                _getTokenByClientCredentialsGrantTypeActionStub.Object,
                _clientCredentialsGrantTypeParameterValidatorStub.Object,
                _revokeTokenParameterValidator.Object,
                _oauthEventSource.Object,
                _revokeTokenActionStub.Object,
                eventPublisher.Object,
                _payloadSerializerStub.Object);
        }
    }
}
