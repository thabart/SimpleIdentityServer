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
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Token
{
    public class GetTokenByClientCredentialsGrantTypeActionFixture
    {
        private Mock<IAuthenticateInstructionGenerator> _authenticateInstructionGeneratorStub;

        private Mock<IAuthenticateClient> _authenticateClientStub;

        private Mock<IClientValidator> _clientValidatorStub;

        private Mock<IGrantedTokenGeneratorHelper> _grantedTokenGeneratorHelperStub;

        private Mock<IScopeValidator> _scopeValidatorStub;

        private Mock<IGrantedTokenRepository> _grantedTokenRepositoryStub;

        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceStub;

        private Mock<IClientCredentialsGrantTypeParameterValidator> _clientCredentialsGrantTypeParameterValidatorStub;

        private Mock<IClientHelper> _clientHelperStub;

        private Mock<IGrantedTokenHelper> _grantedTokenHelperStub;

        private IGetTokenByClientCredentialsGrantTypeAction _getTokenByClientCredentialsGrantTypeAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(null, null));
        }

        [Fact]
        public void When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var message = "message";
            InitializeFakeObjects();
            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
            {
                Scope = "scope"
            };
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), out message))
                .Returns(() => null);

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidClient);
            Assert.True(exception.Message == message);
        }

        [Fact]
        public void When_ClientCredentialGrantType_Is_Not_Supported_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var message = "message";
            InitializeFakeObjects();
            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
            {
                Scope = "scope"
            };
            var client = new Client
            {
                GrantTypes = new List<GrantType>
                {
                    GrantType.password
                }
            };
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), out message))
                .Returns(client);

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, client.ClientId, GrantType.client_credentials));
        }

        [Fact]
        public void When_TokenResponseType_Is_Not_Supported_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var message = "message";
            InitializeFakeObjects();
            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
            {
                Scope = "scope"
            };
            var client = new Client
            {
                GrantTypes = new List<GrantType>
                {
                    GrantType.client_credentials
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.code
                }
            };
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), out message))
                .Returns(client);

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidClient);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType, client.ClientId, ResponseType.token));
        }

        [Fact]
        public void When_Scope_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var message = "message";
            var messageDescription = "message_description";
            InitializeFakeObjects();
            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
            {
                Scope = "scope"
            };
            var client = new Client
            {
                GrantTypes = new List<GrantType>
                {
                    GrantType.client_credentials
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.token
                }
            };
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), out message))
                .Returns(client);
            _scopeValidatorStub.Setup(s => s.IsScopesValid(It.IsAny<string>(), It.IsAny<Client>(), out messageDescription))
                .Returns(() => null);

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidScope);
            Assert.True(exception.Message == messageDescription);
        }

        #endregion
        
        #region Happy paths

        [Fact]
        public void When_Access_Is_Granted_Then_Token_Is_Returned()
        {
            // ARRANGE
            const string scope = "valid_scope";
            const string clientId = "client_id";
            const string accessToken = "access_token";
            var message = "message";
            var messageDescription = "message_description";
            var scopes = new List<string> { scope };
            InitializeFakeObjects();
            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
            {
                Scope = scope
            };
            var client = new Client
            {
                GrantTypes = new List<GrantType>
                {
                    GrantType.client_credentials
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.token
                },
                ClientId = clientId
            };
            var grantedToken = new GrantedToken
            {
                ClientId = clientId,
                AccessToken = accessToken,
                IdTokenPayLoad = new Jwt.JwsPayload()
            };
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), out message))
                .Returns(client);
            _scopeValidatorStub.Setup(s => s.IsScopesValid(It.IsAny<string>(), It.IsAny<Client>(), out messageDescription))
                .Returns(scopes);
            _grantedTokenHelperStub.Setup(g => g.GetValidGrantedToken(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns((GrantedToken)null);
            _grantedTokenGeneratorHelperStub.Setup(g => g.GenerateToken(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns(grantedToken);

            // ACT
            var result = _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null);

            // ASSERTS
            _simpleIdentityServerEventSourceStub.Verify(s => s.GrantAccessToClient(clientId, accessToken, scope));
            Assert.NotNull(result);
            Assert.True(result.ClientId == clientId);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _authenticateInstructionGeneratorStub = new Mock<IAuthenticateInstructionGenerator>();
            _authenticateClientStub = new Mock<IAuthenticateClient>();
            _clientValidatorStub = new Mock<IClientValidator>();
            _grantedTokenGeneratorHelperStub = new Mock<IGrantedTokenGeneratorHelper>();
            _scopeValidatorStub = new Mock<IScopeValidator>();
            _grantedTokenRepositoryStub = new Mock<IGrantedTokenRepository>();
            _simpleIdentityServerEventSourceStub = new Mock<ISimpleIdentityServerEventSource>();
            _clientCredentialsGrantTypeParameterValidatorStub = new Mock<IClientCredentialsGrantTypeParameterValidator>();
            _clientHelperStub = new Mock<IClientHelper>();
            _grantedTokenHelperStub = new Mock<IGrantedTokenHelper>();
            _getTokenByClientCredentialsGrantTypeAction = new GetTokenByClientCredentialsGrantTypeAction(
                _authenticateInstructionGeneratorStub.Object,
                _authenticateClientStub.Object,
                _clientValidatorStub.Object,
                _grantedTokenGeneratorHelperStub.Object,
                _scopeValidatorStub.Object,
                _grantedTokenRepositoryStub.Object,
                _simpleIdentityServerEventSourceStub.Object,
                _clientCredentialsGrantTypeParameterValidatorStub.Object,
                _clientHelperStub.Object,
                _grantedTokenHelperStub.Object);
        }
    }
}
