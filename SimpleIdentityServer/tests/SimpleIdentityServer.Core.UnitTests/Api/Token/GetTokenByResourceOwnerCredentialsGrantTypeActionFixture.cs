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
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.OAuth.Logging;
using SimpleIdentityServer.Store;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Token
{
    public sealed class GetTokenByResourceOwnerCredentialsGrantTypeActionFixture
    {
        private Mock<IGrantedTokenGeneratorHelper> _grantedTokenGeneratorHelperFake;
        private Mock<IScopeValidator> _scopeValidatorFake;
        private Mock<IResourceOwnerAuthenticateHelper> _resourceOwnerAuthenticateHelperFake;
        private Mock<IOAuthEventSource> _oauthEventSource;
        private Mock<IAuthenticateClient> _authenticateClientFake;
        private Mock<IJwtGenerator> _jwtGeneratorFake;
        private Mock<IAuthenticateInstructionGenerator> _authenticateInstructionGeneratorStub;
        private Mock<IClientRepository> _clientRepositoryStub;
        private Mock<IClientHelper> _clientHelperStub;
        private Mock<IGrantedTokenHelper> _grantedTokenHelperStub;
        private Mock<ITokenStore> _tokenStoreStub;
        private IGetTokenByResourceOwnerCredentialsGrantTypeAction _getTokenByResourceOwnerCredentialsGrantTypeAction;

        #region Exceptions

        [Fact]
        public async Task When_Passing_No_Request_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(null, null));
        }

        [Fact]
        public async Task When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
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
            _authenticateClientFake.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>()))
                .Returns(() => Task.FromResult(new AuthenticationResult(null, "error")));
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(() => Task.FromResult((Core.Common.Models.Client)null));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(resourceOwnerGrantTypeParameter, null));
            _oauthEventSource.Verify(s => s.Info("error"));
            Assert.True(exception.Code == ErrorCodes.InvalidClient);
            Assert.True(exception.Message == "error");
        }

        [Fact]
        public async Task When_Client_GrantType_Is_Not_Valid_Then_Exception_Is_Thrown()
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
            _authenticateClientFake.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>()))
                .Returns(() => Task.FromResult(new AuthenticationResult(new Core.Common.Models.Client
                {
                    ClientId = "id",
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.authorization_code
                    }
                }, null)));
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(() => Task.FromResult((Core.Common.Models.Client)null));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(resourceOwnerGrantTypeParameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidClient);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, "id", GrantType.password));
        }

        [Fact]
        public async Task When_Client_ResponseTypes_Are_Not_Valid_Then_Exception_Is_Thrown()
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
            _authenticateClientFake.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>()))
                .Returns(() => Task.FromResult(new AuthenticationResult(new Core.Common.Models.Client
                {
                    ClientId = "id",
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.password
                    }
                }, null)));
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(() => Task.FromResult((Core.Common.Models.Client)null));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(resourceOwnerGrantTypeParameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidClient);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType, "id", "token id_token"));
        }

        [Fact]
        public async Task When_The_Resource_Owner_Is_Not_Valid_Then_Exception_Is_Thrown()
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
            var client = new AuthenticationResult(new Core.Common.Models.Client
            {
                ClientId = "id",
                GrantTypes = new List<GrantType>
                {
                    GrantType.password
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.id_token,
                    ResponseType.token
                }
            }, null);            
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientFake.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>()))
                .Returns(() => Task.FromResult(client));
            _resourceOwnerAuthenticateHelperFake.Setup(
                r => r.Authenticate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(() => Task.FromResult((ResourceOwner)null));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(resourceOwnerGrantTypeParameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message == ErrorDescriptions.ResourceOwnerCredentialsAreNotValid);
        }

        [Fact]
        public async Task When_Passing_A_Not_Allowed_Scopes_Then_Exception_Is_Throw()
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
            var client = new AuthenticationResult(new Core.Common.Models.Client
            {
                ClientId = "id",
                GrantTypes = new List<GrantType>
                {
                    GrantType.password
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.id_token,
                    ResponseType.token
                }
            }, null);
            var resourceOwner = new ResourceOwner();
            
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientFake.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>()))
                .Returns(() => Task.FromResult(client));
            _resourceOwnerAuthenticateHelperFake.Setup(
                r => r.Authenticate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(() => Task.FromResult(resourceOwner));
            _scopeValidatorFake.Setup(s => s.Check(It.IsAny<string>(), It.IsAny<Core.Common.Models.Client>()))
                .Returns(() => new ScopeValidationResult(false));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(resourceOwnerGrantTypeParameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidScope);
        }

        #endregion

        #region Happy path

        [Fact]
        public async Task When_Requesting_An_AccessToken_For_An_Authenticated_User_Then_AccessToken_Is_Granted()
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
            var client = new AuthenticationResult(new Core.Common.Models.Client
            {
                ClientId = clientId,
                GrantTypes = new List<GrantType>
                {
                    GrantType.password
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.id_token,
                    ResponseType.token
                }
            }, null);
            var resourceOwner = new ResourceOwner();
            var userInformationJwsPayload = new JwsPayload();
            var grantedToken = new GrantedToken
            {
                AccessToken = accessToken,
                IdTokenPayLoad = new JwsPayload()
            };

            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientFake.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>()))
                .Returns(() => Task.FromResult(client));
            _resourceOwnerAuthenticateHelperFake.Setup(
                r => r.Authenticate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(() => Task.FromResult(resourceOwner));
            _scopeValidatorFake.Setup(s => s.Check(It.IsAny<string>(), It.IsAny<Core.Common.Models.Client>()))
                .Returns(() => new ScopeValidationResult(true)
                {
                    Scopes = new List<string> { invalidScope }
                });
            _jwtGeneratorFake.Setup(
                j => j.GenerateUserInfoPayloadForScopeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthorizationParameter>()))
                .Returns(() => Task.FromResult(userInformationJwsPayload));
            _grantedTokenHelperStub.Setup(g => g.GetValidGrantedTokenAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns(Task.FromResult((GrantedToken)null));
            _grantedTokenGeneratorHelperFake.Setup(g => g.GenerateTokenAsync(It.IsAny<Core.Common.Models.Client>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns(Task.FromResult(grantedToken));

            // ACT
            await _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(resourceOwnerGrantTypeParameter, null);

            // ASSERT
            _tokenStoreStub.Verify(g => g.AddToken(grantedToken));
            _oauthEventSource.Verify(s => s.GrantAccessToClient(clientId, accessToken, invalidScope));
            _clientHelperStub.Verify(c => c.GenerateIdTokenAsync(It.IsAny<Core.Common.Models.Client>(), It.IsAny<JwsPayload>()));
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _grantedTokenGeneratorHelperFake = new Mock<IGrantedTokenGeneratorHelper>();
            _scopeValidatorFake = new Mock<IScopeValidator>();
            _resourceOwnerAuthenticateHelperFake = new Mock<IResourceOwnerAuthenticateHelper>();
            _oauthEventSource = new Mock<IOAuthEventSource>();
            _authenticateClientFake = new Mock<IAuthenticateClient>();
            _jwtGeneratorFake = new Mock<IJwtGenerator>();
            _authenticateInstructionGeneratorStub = new Mock<IAuthenticateInstructionGenerator>();
            _clientRepositoryStub = new Mock<IClientRepository>();
            _clientHelperStub = new Mock<IClientHelper>();
            _grantedTokenHelperStub = new Mock<IGrantedTokenHelper>();
            _tokenStoreStub = new Mock<ITokenStore>();

            _getTokenByResourceOwnerCredentialsGrantTypeAction = new GetTokenByResourceOwnerCredentialsGrantTypeAction(
                _grantedTokenGeneratorHelperFake.Object,
                _scopeValidatorFake.Object,
                _resourceOwnerAuthenticateHelperFake.Object,
                _oauthEventSource.Object,
                _authenticateClientFake.Object,
                _jwtGeneratorFake.Object,
                _authenticateInstructionGeneratorStub.Object,
                _clientRepositoryStub.Object,
                _clientHelperStub.Object,
                _tokenStoreStub.Object,
                _grantedTokenHelperStub.Object);
        }
    }
}
