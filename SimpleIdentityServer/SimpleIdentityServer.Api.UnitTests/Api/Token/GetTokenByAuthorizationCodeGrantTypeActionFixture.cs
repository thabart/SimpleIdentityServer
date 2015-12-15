using Moq;
using NUnit.Framework;
using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System;

namespace SimpleIdentityServer.Api.UnitTests.Api.Token
{
    [TestFixture]
    public sealed class GetTokenByAuthorizationCodeGrantTypeActionFixture
    {
        private Mock<IClientValidator> _clientValidatorFake;

        private Mock<IAuthorizationCodeRepository> _authorizationCodeRepositoryFake;

        private Mock<ISimpleIdentityServerConfigurator> _simpleIdentityServerConfiguratorFake;

        private Mock<IGrantedTokenGeneratorHelper> _grantedTokenGeneratorHelperFake;

        private Mock<IGrantedTokenRepository> _grantedTokenRepositoryFake;

        private Mock<IAuthenticateClient> _authenticateClientFake;

        private Mock<IJwtGenerator> _jwtGeneratorFake;

        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceFake;

        private IGetTokenByAuthorizationCodeGrantTypeAction _getTokenByAuthorizationCodeGrantTypeAction;

        [Test]
        public void When_Passing_Empty_Request_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => 
                _getTokenByAuthorizationCodeGrantTypeAction.Execute(null, null));
        }

        [Test]
        public void When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientId = "clientId",
                ClientSecret = "clientSecret"
            };

            string errorMessage;
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(),
                out errorMessage))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => 
                _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, null));
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidClient);
        }

        [Test]
        public void When_Authorization_Code_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientId = "clientId",
                ClientSecret = "clientSecret"
            };
            var client = new Client();

            string errorMessage;
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(),
                out errorMessage))
                .Returns(() => client);
            _authorizationCodeRepositoryFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() =>
                _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, null));
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidGrant);
            Assert.IsTrue(exception.Message == ErrorDescriptions.TheAuthorizationCodeIsNotCorrect);
        }

        [Test]
        public void When_Granted_Client_Is_Not_The_Same_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientId = "notCorrectClientId",
                ClientSecret = "clientSecret"
            };
            var client = new Client
            {
                ClientId = "notCorrectClientId"
            };
            var authorizationCode = new AuthorizationCode
            {
                ClientId = "clientId"
            };

            string errorMessage;
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(),
                out errorMessage))
                .Returns(() => client);
            _authorizationCodeRepositoryFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(() => authorizationCode);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() =>
                _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, null));
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidGrant);
            Assert.IsTrue(exception.Message == 
                string.Format(ErrorDescriptions.TheAuthorizationCodeHasNotBeenIssuedForTheGivenClientId,
                        authorizationCodeGrantTypeParameter.ClientId));
        }

        [Test]
        public void When_Redirect_Uri_Is_Not_The_Same_Then_Exception_Is_Thrown()
        {            
            // ARRANGE
            InitializeFakeObjects();
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientId = "clientId",
                ClientSecret = "clientSecret",
                RedirectUri = "notCorrectRedirectUri"
            };
            var client = new Client();
            var authorizationCode = new AuthorizationCode
            {
                ClientId = "clientId",
                RedirectUri = "redirectUri"
            };

            string errorMessage;
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(),
                out errorMessage))
                .Returns(client);
            _authorizationCodeRepositoryFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(authorizationCode);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() =>
                _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, null));
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidGrant);
            Assert.IsTrue(exception.Message == ErrorDescriptions.TheRedirectionUrlIsNotTheSame);
        }

        [Test]
        public void When_The_Authorization_Code_Has_Expired_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientSecret = "clientSecret",
                RedirectUri = "redirectUri",
                ClientId = "clientId",
            };
            var client = new Client();
            var authorizationCode = new AuthorizationCode
            {
                ClientId = "clientId",
                RedirectUri = "redirectUri",
                CreateDateTime = DateTime.UtcNow.AddSeconds(-30)
            };

            string errorMessage;
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(),
                out errorMessage))
                .Returns(client);
            _authorizationCodeRepositoryFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(authorizationCode);
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetAuthorizationCodeValidityPeriodInSeconds())
                .Returns(2);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() =>
                _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, null));
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidGrant);
            Assert.IsTrue(exception.Message == ErrorDescriptions.TheAuthorizationCodeIsObsolete);
        }

        [Test]
        public void When_RedirectUri_Is_Different_From_The_One_Hold_By_The_Client_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientSecret = "clientSecret",
                RedirectUri = "redirectUri",
                ClientId = "clientId",
            };
            var client = new Client();
            var authorizationCode = new AuthorizationCode
            {
                ClientId = "clientId",
                RedirectUri = "redirectUri",
                CreateDateTime = DateTime.UtcNow
            };

            string errorMessage;
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(),
                out errorMessage))
                .Returns(client);
            _authorizationCodeRepositoryFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(authorizationCode);
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetAuthorizationCodeValidityPeriodInSeconds())
                .Returns(3000);
            _clientValidatorFake.Setup(c => c.ValidateRedirectionUrl(It.IsAny<string>(), It.IsAny<Client>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() =>
                _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, null));
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidGrant);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.RedirectUrlIsNotValid, "redirectUri"));
        }

        [Test]
        public void When_Requesting_Token_And_There_Is_No_Valid_Granted_Token_Then_Grant_A_New_One()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string accessToken = "accessToken";
            const string identityToken = "identityToken";
            const string clientId = "clientId";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientSecret = "clientSecret",
                RedirectUri = "redirectUri",
                ClientId = clientId
            };
            var client = new Client();
            var authorizationCode = new AuthorizationCode
            {
                ClientId = clientId,
                RedirectUri = "redirectUri",
                CreateDateTime = DateTime.UtcNow
            };
            var grantedToken = new GrantedToken
            {
                AccessToken = accessToken,
                IdToken = identityToken
            };

            string errorMessage;
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(),
                out errorMessage))
                .Returns(client);
            _authorizationCodeRepositoryFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(authorizationCode);
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetAuthorizationCodeValidityPeriodInSeconds())
                .Returns(3000);
            _clientValidatorFake.Setup(c => c.ValidateRedirectionUrl(It.IsAny<string>(), It.IsAny<Client>()))
                .Returns("redirectUri");
            _grantedTokenRepositoryFake.Setup(g => g.GetToken(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns(() => null);
            _grantedTokenGeneratorHelperFake.Setup(g => g.GenerateToken(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns(grantedToken); ;

            // ACT
            var result = _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, null);

            // ASSERTS
            _grantedTokenRepositoryFake.Verify(g => g.Insert(grantedToken));
            _simpleIdentityServerEventSourceFake.Verify(s => s.GrantAccessToClient(
                clientId,
                accessToken,
                identityToken));
            Assert.IsTrue(result.AccessToken == accessToken);
        }

        private void InitializeFakeObjects()
        {
            _clientValidatorFake = new Mock<IClientValidator>();
            _authorizationCodeRepositoryFake = new Mock<IAuthorizationCodeRepository>();
            _simpleIdentityServerConfiguratorFake = new Mock<ISimpleIdentityServerConfigurator>();
            _grantedTokenGeneratorHelperFake = new Mock<IGrantedTokenGeneratorHelper>();
            _grantedTokenRepositoryFake = new Mock<IGrantedTokenRepository>();
            _authenticateClientFake = new Mock<IAuthenticateClient>();
            _jwtGeneratorFake = new Mock<IJwtGenerator>();
            _simpleIdentityServerConfiguratorFake = new Mock<ISimpleIdentityServerConfigurator>();
            _simpleIdentityServerEventSourceFake = new Mock<ISimpleIdentityServerEventSource>();

            _getTokenByAuthorizationCodeGrantTypeAction = new GetTokenByAuthorizationCodeGrantTypeAction(
                _clientValidatorFake.Object,
                _authorizationCodeRepositoryFake.Object,
                _simpleIdentityServerConfiguratorFake.Object,
                _grantedTokenGeneratorHelperFake.Object,
                _grantedTokenRepositoryFake.Object,
                _authenticateClientFake.Object,
                _jwtGeneratorFake.Object,
                _simpleIdentityServerEventSourceFake.Object); 
        }
    }
}
