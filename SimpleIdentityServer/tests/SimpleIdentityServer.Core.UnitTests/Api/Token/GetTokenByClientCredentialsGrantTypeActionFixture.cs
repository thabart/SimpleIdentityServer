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
using System.Threading.Tasks;
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
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(null, null));
        }

        [Fact]
        public async Task When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
            {
                Scope = "scope"
            };
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>()))
                .Returns(() => Task.FromResult(new AuthenticationResult(null, null)));

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidClient);
        }

        [Fact]
        public async Task When_ClientCredentialGrantType_Is_Not_Supported_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
            {
                Scope = "scope"
            };
            var client = new AuthenticationResult(new Models.Client
            {
                GrantTypes = new List<GrantType>
                {
                    GrantType.password
                }
            }, null);
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>()))
                .Returns(Task.FromResult(client));

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, client.Client.ClientId, GrantType.client_credentials));
        }

        [Fact]
        public async Task When_TokenResponseType_Is_Not_Supported_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
            {
                Scope = "scope"
            };
            var client = new AuthenticationResult(new Models.Client
            {
                GrantTypes = new List<GrantType>
                {
                    GrantType.client_credentials
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.code
                }
            }, null);
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>()))
                .Returns(Task.FromResult(client));

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidClient);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType, client.Client.ClientId, ResponseType.token));
        }

        [Fact]
        public async Task When_Scope_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var messageDescription = "message_description";
            InitializeFakeObjects();
            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
            {
                Scope = "scope"
            };
            var client = new AuthenticationResult(new Models.Client
            {
                GrantTypes = new List<GrantType>
                {
                    GrantType.client_credentials
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.token
                }
            }, null);
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>()))
                .Returns(Task.FromResult(client));
            _clientValidatorStub.Setup(c => c.GetRedirectionUrls(It.IsAny<Client>(), It.IsAny<string[]>())).Returns(new string[0]);
            _scopeValidatorStub.Setup(s => s.Check(It.IsAny<string>(), It.IsAny<Models.Client>()))
                .Returns(() => new ScopeValidationResult(false)
                {
                    ErrorMessage = messageDescription
                });

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidScope);
            Assert.True(exception.Message == messageDescription);
        }

        #endregion
        
        #region Happy paths

        [Fact]
        public async Task When_Access_Is_Granted_Then_Token_Is_Returned()
        {
            // ARRANGE
            const string scope = "valid_scope";
            const string clientId = "client_id";
            const string accessToken = "access_token";
            var messageDescription = "message_description";
            var scopes = new List<string> { scope };
            InitializeFakeObjects();
            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
            {
                Scope = scope
            };
            var client = new AuthenticationResult(new Models.Client
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
            }, null);
            var grantedToken = new GrantedToken
            {
                ClientId = clientId,
                AccessToken = accessToken,
                IdTokenPayLoad = new Jwt.JwsPayload()
            };
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>()))
                .Returns(Task.FromResult(client));
            _scopeValidatorStub.Setup(s => s.Check(It.IsAny<string>(), It.IsAny<Models.Client>()))
                .Returns(() => new ScopeValidationResult(true)
                {
                    Scopes = scopes
                });
            _grantedTokenHelperStub.Setup(g => g.GetValidGrantedTokenAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns(Task.FromResult((GrantedToken)null));
            _grantedTokenGeneratorHelperStub.Setup(g => g.GenerateTokenAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns(Task.FromResult(grantedToken));

            // ACT
            var result = await _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null);

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
