#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Moq;
using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System;
using System.Net.Http.Headers;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Token
{
    public sealed class GetTokenByAuthorizationCodeGrantTypeActionFixture
    {
        private Mock<IClientValidator> _clientValidatorFake;
        private Mock<IAuthorizationCodeRepository> _authorizationCodeRepositoryFake;
        private Mock<IConfigurationService> _simpleIdentityServerConfiguratorFake;
        private Mock<IGrantedTokenGeneratorHelper> _grantedTokenGeneratorHelperFake;
        private Mock<IGrantedTokenRepository> _grantedTokenRepositoryFake;
        private Mock<IAuthenticateClient> _authenticateClientFake;
        private Mock<IClientHelper> _clientHelper;
        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceFake;
        private Mock<IAuthenticateInstructionGenerator> _authenticateInstructionGeneratorStub;
        private Mock<IGrantedTokenHelper> _grantedTokenHelperStub;
        private IGetTokenByAuthorizationCodeGrantTypeAction _getTokenByAuthorizationCodeGrantTypeAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Empty_Request_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => 
                _getTokenByAuthorizationCodeGrantTypeAction.Execute(null, null));
        }

        [Fact]
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
            
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>()))
                .Returns(() => new AuthenticationResult(null, null));

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => 
                _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidClient);
        }

        [Fact]
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
            var client = new AuthenticationResult(new Client(), null);

            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>()))
                .Returns(() => client);
            _authorizationCodeRepositoryFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() =>
                _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == ErrorDescriptions.TheAuthorizationCodeIsNotCorrect);
        }

        [Fact]
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

            var result = new AuthenticationResult(new Models.Client { ClientId = "notCorrectClientId" } , null);
            var authorizationCode = new AuthorizationCode
            {
                ClientId = "clientId"
            };
            
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>()))
                .Returns(() => result);
            _authorizationCodeRepositoryFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(() => authorizationCode);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() =>
                _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == 
                string.Format(ErrorDescriptions.TheAuthorizationCodeHasNotBeenIssuedForTheGivenClientId,
                        authorizationCodeGrantTypeParameter.ClientId));
        }

        [Fact]
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

            var result = new AuthenticationResult(new Models.Client { ClientId = "clientId" }, null);
            var authorizationCode = new AuthorizationCode
            {
                ClientId = "clientId",
                RedirectUri = "redirectUri"
            };

            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>()))
                .Returns(result);
            _authorizationCodeRepositoryFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(authorizationCode);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() =>
                _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == ErrorDescriptions.TheRedirectionUrlIsNotTheSame);
        }

        [Fact]
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
            var result = new AuthenticationResult(new Models.Client { ClientId = "clientId" }, null);
            var authorizationCode = new AuthorizationCode
            {
                ClientId = "clientId",
                RedirectUri = "redirectUri",
                CreateDateTime = DateTime.UtcNow.AddSeconds(-30)
            };

            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>()))
                .Returns(result);
            _authorizationCodeRepositoryFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(authorizationCode);
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetAuthorizationCodeValidityPeriodInSeconds())
                .Returns(2);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() =>
                _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == ErrorDescriptions.TheAuthorizationCodeIsObsolete);
        }

        [Fact]
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
            var result = new AuthenticationResult(new Models.Client { ClientId = "clientId" }, null);
            var authorizationCode = new AuthorizationCode
            {
                ClientId = "clientId",
                RedirectUri = "redirectUri",
                CreateDateTime = DateTime.UtcNow
            };

            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>()))
                .Returns(result);
            _authorizationCodeRepositoryFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(authorizationCode);
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetAuthorizationCodeValidityPeriodInSeconds())
                .Returns(3000);
            _clientValidatorFake.Setup(c => c.ValidateRedirectionUrl(It.IsAny<string>(), It.IsAny<Models.Client>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() =>
                _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.RedirectUrlIsNotValid, "redirectUri"));
        }

        #endregion

        #region Happy paths

        [Fact]
        public void When_Requesting_An_Existed_Granted_Token_Then_Check_Id_Token_Is_Signed_And_Encrypted()
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

            var result = new AuthenticationResult(new Models.Client {
                ClientId = "clientId",
                IdTokenSignedResponseAlg = Jwt.Constants.JwsAlgNames.RS256,
                IdTokenEncryptedResponseAlg = Jwt.Constants.JweAlgNames.RSA1_5,
                IdTokenEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256
            }, null);
            var authorizationCode = new AuthorizationCode
            {
                ClientId = clientId,
                RedirectUri = "redirectUri",
                CreateDateTime = DateTime.UtcNow
            };
            var grantedToken = new GrantedToken
            {
                ClientId = clientId,
                AccessToken = accessToken,
                IdToken = identityToken,
                IdTokenPayLoad = new JwsPayload()
            };

            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>()))
                .Returns(result);
            _authorizationCodeRepositoryFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(authorizationCode);
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetAuthorizationCodeValidityPeriodInSeconds())
                .Returns(3000);
            _clientValidatorFake.Setup(c => c.ValidateRedirectionUrl(It.IsAny<string>(), It.IsAny<Models.Client>()))
                .Returns("redirectUri");
            _clientValidatorFake.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(result.Client);
            _grantedTokenHelperStub.Setup(g => g.GetValidGrantedToken(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns(grantedToken);

            // ACT
            _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, null);

            // ASSERTS
            _clientHelper.Verify(j => j.GenerateIdToken(clientId, It.IsAny<JwsPayload>()));
        }

        [Fact]
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
            var authResult = new AuthenticationResult(new Models.Client
            {
                ClientId = clientId
            }, null);
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

            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>()))
                .Returns(authResult);
            _authorizationCodeRepositoryFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(authorizationCode);
            _simpleIdentityServerConfiguratorFake.Setup(s => s.GetAuthorizationCodeValidityPeriodInSeconds())
                .Returns(3000);
            _clientValidatorFake.Setup(c => c.ValidateRedirectionUrl(It.IsAny<string>(), It.IsAny<Models.Client>()))
                .Returns("redirectUri");
            _grantedTokenHelperStub.Setup(g => g.GetValidGrantedToken(It.IsAny<string>(),
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
            Assert.True(result.AccessToken == accessToken);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _clientValidatorFake = new Mock<IClientValidator>();
            _authorizationCodeRepositoryFake = new Mock<IAuthorizationCodeRepository>();
            _grantedTokenGeneratorHelperFake = new Mock<IGrantedTokenGeneratorHelper>();
            _grantedTokenRepositoryFake = new Mock<IGrantedTokenRepository>();
            _authenticateClientFake = new Mock<IAuthenticateClient>();
            _clientHelper = new Mock<IClientHelper>();
            _simpleIdentityServerConfiguratorFake = new Mock<IConfigurationService>();
            _simpleIdentityServerEventSourceFake = new Mock<ISimpleIdentityServerEventSource>();
            _authenticateInstructionGeneratorStub = new Mock<IAuthenticateInstructionGenerator>();
            _grantedTokenHelperStub = new Mock<IGrantedTokenHelper>();
            _getTokenByAuthorizationCodeGrantTypeAction = new GetTokenByAuthorizationCodeGrantTypeAction(
                _clientValidatorFake.Object,
                _authorizationCodeRepositoryFake.Object,
                _simpleIdentityServerConfiguratorFake.Object,
                _grantedTokenGeneratorHelperFake.Object,
                _grantedTokenRepositoryFake.Object,
                _authenticateClientFake.Object,
                _clientHelper.Object,
                _simpleIdentityServerEventSourceFake.Object,
                _authenticateInstructionGeneratorStub.Object,
                _grantedTokenHelperStub.Object); 
        }
    }
}
