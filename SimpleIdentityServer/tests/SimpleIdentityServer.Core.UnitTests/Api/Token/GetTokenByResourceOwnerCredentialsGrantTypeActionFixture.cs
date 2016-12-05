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

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Moq;
using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Authenticate;
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
using Xunit;
using System.Net.Http.Headers;
using SimpleIdentityServer.Core.Services;

namespace SimpleIdentityServer.Core.UnitTests.Api.Token
{
    public sealed class GetTokenByResourceOwnerCredentialsGrantTypeActionFixture
    {
        private Mock<IGrantedTokenRepository> _grantedTokenRepositoryFake;
        private Mock<IGrantedTokenGeneratorHelper> _grantedTokenGeneratorHelperFake;
        private Mock<IScopeValidator> _scopeValidatorFake;
        private Mock<IAuthenticateResourceOwnerService> _resourceOwnerValidatorFake;
        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceFake;
        private Mock<IAuthenticateClient> _authenticateClientFake;
        private Mock<IJwtGenerator> _jwtGeneratorFake;
        private Mock<IAuthenticateInstructionGenerator> _authenticateInstructionGeneratorStub;
        private Mock<IClientRepository> _clientRepositoryStub;
        private Mock<IClientHelper> _clientHelperStub;
        private Mock<IGrantedTokenHelper> _grantedTokenHelperStub;
        private IGetTokenByResourceOwnerCredentialsGrantTypeAction _getTokenByResourceOwnerCredentialsGrantTypeAction;

        #region Exceptions

        [Fact]
        public void When_Passing_No_Request_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(null, null));
        }

        [Fact]
        public void When_AnonymousClient_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientAssertion = "clientAssertion";
            const string clientAssertionType = "clientAssertionType";
            const string clientId = "clientId";
            const string clientSecret = "clientSecret";
            var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
            {
                ClientAssertion = clientAssertion,
                ClientAssertionType = clientAssertionType,
                ClientId = clientId,
                ClientSecret = clientSecret
            };
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>()))
                .Returns(() => new AuthenticationResult(null, null));
            _clientRepositoryStub.Setup(c => c.GetClientById(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(resourceOwnerGrantTypeParameter, null));
            _simpleIdentityServerEventSourceFake.Verify(s => s.Info(ErrorDescriptions.TheClientCannotBeAuthenticated));
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.ClientIsNotValid, Constants.AnonymousClientId));
        }
        
        [Fact]
        public void When_The_Resource_Owner_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientAssertion = "clientAssertion";
            const string clientAssertionType = "clientAssertionType";
            const string clientId = "clientId";
            const string clientSecret = "clientSecret";
            var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
            {
                ClientAssertion = clientAssertion,
                ClientAssertionType = clientAssertionType,
                ClientId = clientId,
                ClientSecret = clientSecret
            };
            var client = new AuthenticationResult(new Models.Client(), null);

            string message;
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>()))
                .Returns(() => client);
            _resourceOwnerValidatorFake.Setup(
                r => r.AuthenticateResourceOwner(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(resourceOwnerGrantTypeParameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == ErrorDescriptions.ResourceOwnerCredentialsAreNotValid);
        }

        [Fact]
        public void When_Passing_A_Not_Allowed_Scopes_Then_Exception_Is_Throw()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientAssertion = "clientAssertion";
            const string clientAssertionType = "clientAssertionType";
            const string clientId = "clientId";
            const string clientSecret = "clientSecret";
            const string invalidScope = "invalidScope";
            var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
            {
                ClientAssertion = clientAssertion,
                ClientAssertionType = clientAssertionType,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = invalidScope
            };
            var client = new AuthenticationResult(new Models.Client(), null);
            var resourceOwner = new ResourceOwner();

            string message;
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>()))
                .Returns(() => client);
            _resourceOwnerValidatorFake.Setup(
                r => r.AuthenticateResourceOwner(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => resourceOwner);
            _scopeValidatorFake.Setup(s => s.IsScopesValid(It.IsAny<string>(), It.IsAny<Models.Client>(), out message))
                .Returns(() => new List<string>());

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(resourceOwnerGrantTypeParameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidScope);
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_Requesting_An_AccessToken_For_An_Authenticated_User_Then_AccessToken_Is_Granted()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientAssertion = "clientAssertion";
            const string clientAssertionType = "clientAssertionType";
            const string clientId = "clientId";
            const string clientSecret = "clientSecret";
            const string invalidScope = "invalidScope";
            const string accessToken = "accessToken";
            var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
            {
                ClientAssertion = clientAssertion,
                ClientAssertionType = clientAssertionType,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = invalidScope
            };
            var client = new AuthenticationResult(new Models.Client
            {
                ClientId = clientId
            }, null);
            var resourceOwner = new ResourceOwner();
            var userInformationJwsPayload = new JwsPayload();
            var grantedToken = new GrantedToken
            {
                AccessToken = accessToken,
                IdTokenPayLoad = new JwsPayload()
            };

            string message;
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientFake.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>()))
                .Returns(() => client);
            _resourceOwnerValidatorFake.Setup(
                r => r.AuthenticateResourceOwner(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => resourceOwner);
            _scopeValidatorFake.Setup(s => s.IsScopesValid(It.IsAny<string>(), It.IsAny<Models.Client>(), out message))
                .Returns(() => new List<string> { invalidScope });
            _jwtGeneratorFake.Setup(
                j => j.GenerateUserInfoPayloadForScope(It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthorizationParameter>()))
                .Returns(() => userInformationJwsPayload);
            _grantedTokenHelperStub.Setup(g => g.GetValidGrantedToken(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns((GrantedToken)null);
            _grantedTokenGeneratorHelperFake.Setup(g => g.GenerateToken(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns(() => grantedToken);

            // ACT
            _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(resourceOwnerGrantTypeParameter, null);

            // ASSERT
            _grantedTokenRepositoryFake.Verify(g => g.Insert(grantedToken));
            _simpleIdentityServerEventSourceFake.Verify(s => s.GrantAccessToClient(clientId, accessToken, invalidScope));
            _clientHelperStub.Verify(c => c.GenerateIdToken(It.IsAny<string>(), It.IsAny<JwsPayload>()));
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _grantedTokenRepositoryFake = new Mock<IGrantedTokenRepository>();
            _grantedTokenGeneratorHelperFake = new Mock<IGrantedTokenGeneratorHelper>();
            _scopeValidatorFake = new Mock<IScopeValidator>();
            _resourceOwnerValidatorFake = new Mock<IAuthenticateResourceOwnerService>();
            _simpleIdentityServerEventSourceFake = new Mock<ISimpleIdentityServerEventSource>();
            _authenticateClientFake = new Mock<IAuthenticateClient>();
            _jwtGeneratorFake = new Mock<IJwtGenerator>();
            _authenticateInstructionGeneratorStub = new Mock<IAuthenticateInstructionGenerator>();
            _clientRepositoryStub = new Mock<IClientRepository>();
            _clientHelperStub = new Mock<IClientHelper>();
            _grantedTokenHelperStub = new Mock<IGrantedTokenHelper>();

            _getTokenByResourceOwnerCredentialsGrantTypeAction = new GetTokenByResourceOwnerCredentialsGrantTypeAction(
                _grantedTokenRepositoryFake.Object,
                _grantedTokenGeneratorHelperFake.Object,
                _scopeValidatorFake.Object,
                _resourceOwnerValidatorFake.Object,
                _simpleIdentityServerEventSourceFake.Object,
                _authenticateClientFake.Object,
                _jwtGeneratorFake.Object,
                _authenticateInstructionGeneratorStub.Object,
                _clientRepositoryStub.Object,
                _clientHelperStub.Object,
                _grantedTokenHelperStub.Object);
        }
    }
}
