using System;
using System.Net.Http.Headers;
using Moq;
using SimpleIdentityServer.Core.Api.Token;
using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using Xunit;

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

        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceFake;

        private Mock<IGetTokenByRefreshTokenGrantTypeAction> _getTokenByRefreshTokenGrantTypeActionFake;

        private Mock<IRefreshTokenGrantTypeParameterValidator> _refreshTokenGrantTypeParameterValidatorFake;

        private ITokenActions _tokenActions;

        [Fact]
        public void When_Passing_No_Request_To_ResourceOwner_Grant_Type_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(null, null));
        }

        [Fact]
        public void When_Requesting_Token_Via_Resource_Owner_Credentials_Grant_Type_Then_Events_Are_Logged()
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
                g => g.Execute(It.IsAny<ResourceOwnerGrantTypeParameter>(), It.IsAny<AuthenticationHeaderValue>()))
                .Returns(grantedToken);

            // ACT
            _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(parameter, null);

            // ASSERTS
            _simpleIdentityServerEventSourceFake.Verify(s => s.StartGetTokenByResourceOwnerCredentials(clientId, userName, password));
            _simpleIdentityServerEventSourceFake.Verify(s => s.EndGetTokenByResourceOwnerCredentials(accessToken, identityToken));
        }

        [Fact]
        public void When_Passing_No_Request_To_AuthorizationCode_Grant_Type_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _tokenActions.GetTokenByAuthorizationCodeGrantType(null, null));
        }

        [Fact]
        public void When_Requesting_Token_Via_AuthorizationCode_Grant_Type_Then_Events_Are_Logged()
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
                g => g.Execute(It.IsAny<AuthorizationCodeGrantTypeParameter>(), It.IsAny<AuthenticationHeaderValue>()))
                .Returns(grantedToken);

            // ACT
            _tokenActions.GetTokenByAuthorizationCodeGrantType(parameter, null);

            // ASSERTS
            _simpleIdentityServerEventSourceFake.Verify(s => s.StartGetTokenByAuthorizationCode(clientId, code));
            _simpleIdentityServerEventSourceFake.Verify(s => s.EndGetTokenByAuthorizationCode(accessToken, identityToken));
        }

        [Fact]
        public void When_Passing_No_Request_To_Refresh_Token_Grant_Type_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _tokenActions.GetTokenByRefreshTokenGrantType(null, null));
        }

        [Fact]
        public void When_Passing_Request_To_Refresh_Token_Grant_Type_Then_Events_Are_Logged()
        {            
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string refreshToken = "refresh_token";
            const string accessToken = "accessToken";
            const string identityToken = "identityToken";
            var parameter = new RefreshTokenGrantTypeParameter
            {
                ClientId = clientId,
                RefreshToken = refreshToken
            };
            var grantedToken = new GrantedToken
            {
                AccessToken = accessToken,
                IdToken = identityToken
            };
            _getTokenByRefreshTokenGrantTypeActionFake.Setup(
                g => g.Execute(It.IsAny<RefreshTokenGrantTypeParameter>()))
                .Returns(grantedToken);

            // ACT
            _tokenActions.GetTokenByRefreshTokenGrantType(parameter, null);

            // ASSERTS
            _simpleIdentityServerEventSourceFake.Verify(s => s.StartGetTokenByRefreshToken(clientId, refreshToken));
            _simpleIdentityServerEventSourceFake.Verify(s => s.EndGetTokenByRefreshToken(accessToken, identityToken));
        }

        private void InitializeFakeObjects()
        {
            _getTokenByResourceOwnerCredentialsGrantTypeActionFake = new Mock<IGetTokenByResourceOwnerCredentialsGrantTypeAction>();
            _getTokenByAuthorizationCodeGrantTypeActionFake = new Mock<IGetTokenByAuthorizationCodeGrantTypeAction>();
            _resourceOwnerGrantTypeParameterValidatorFake = new Mock<IResourceOwnerGrantTypeParameterValidator>();
            _authorizationCodeGrantTypeParameterTokenEdptValidatorFake = new Mock<IAuthorizationCodeGrantTypeParameterTokenEdpValidator>();
            _simpleIdentityServerEventSourceFake = new Mock<ISimpleIdentityServerEventSource>();
            _getTokenByRefreshTokenGrantTypeActionFake = new Mock<IGetTokenByRefreshTokenGrantTypeAction>();
            _refreshTokenGrantTypeParameterValidatorFake = new Mock<IRefreshTokenGrantTypeParameterValidator>();
            _tokenActions = new TokenActions(_getTokenByResourceOwnerCredentialsGrantTypeActionFake.Object,
                _getTokenByAuthorizationCodeGrantTypeActionFake.Object,
                _resourceOwnerGrantTypeParameterValidatorFake.Object,
                _authorizationCodeGrantTypeParameterTokenEdptValidatorFake.Object,
                _refreshTokenGrantTypeParameterValidatorFake.Object,
                _getTokenByRefreshTokenGrantTypeActionFake.Object,
                _simpleIdentityServerEventSourceFake.Object);
        }
    }
}
