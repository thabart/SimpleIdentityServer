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
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.OAuth.Logging;
using SimpleIdentityServer.Store;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Token
{
    public sealed class GetTokenByRefreshTokenGrantTypeActionFixture
    {
        private Mock<IClientHelper> _clientHelperFake;
        private Mock<IOAuthEventSource> _oauthEventSource;
        private Mock<IGrantedTokenGeneratorHelper> _grantedTokenGeneratorHelperStub;
        private Mock<ITokenStore> _tokenStoreStub;
        private Mock<IJwtGenerator> _jwtGeneratorStub;
        private Mock<IAuthenticateInstructionGenerator> _authenticateInstructionGeneratorStub;
        private Mock<IAuthenticateClient> _authenticateClientStub;
        private IGetTokenByRefreshTokenGrantTypeAction _getTokenByRefreshTokenGrantTypeAction;

        #region Exceptions

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getTokenByRefreshTokenGrantTypeAction.Execute(null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RefreshTokenGrantTypeParameter();
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>())).Returns(Task.FromResult(new AuthenticationResult(null, "error")));

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<IdentityServerException>(() => _getTokenByRefreshTokenGrantTypeAction.Execute(parameter, null, null)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidClient);
            Assert.True(ex.Message == "error");
        }

        [Fact]
        public async Task When_Client_Doesnt_Support_GrantType_RefreshToken_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            var parameter = new RefreshTokenGrantTypeParameter();
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>())).Returns(Task.FromResult(new AuthenticationResult(new Core.Common.Models.Client
            {
                ClientId = "id",
                GrantTypes = new System.Collections.Generic.List<GrantType>
                {
                    GrantType.authorization_code
                }
            }, null)));
            
            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<IdentityServerException>(() => _getTokenByRefreshTokenGrantTypeAction.Execute(parameter, null, null)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidClient);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, "id", GrantType.refresh_token));
        }

        [Fact]
        public async Task When_Passing_Invalid_Refresh_Token_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            var parameter = new RefreshTokenGrantTypeParameter();
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>())).Returns(Task.FromResult(new AuthenticationResult(new Core.Common.Models.Client
            {
                ClientId = "id",
                GrantTypes = new System.Collections.Generic.List<GrantType>
                {
                    GrantType.refresh_token
                }
            }, null)));
            _tokenStoreStub.Setup(g => g.GetRefreshToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<IdentityServerException>(() => _getTokenByRefreshTokenGrantTypeAction.Execute(parameter, null, null)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidGrant);
            Assert.True(ex.Message == ErrorDescriptions.TheRefreshTokenIsNotValid);
        }

        [Fact]
        public async Task When_RefreshToken_Is_Not_Issued_By_The_Same_Client_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            var parameter = new RefreshTokenGrantTypeParameter();
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>())).Returns(Task.FromResult(new AuthenticationResult(new Core.Common.Models.Client
            {
                ClientId = "id",
                GrantTypes = new System.Collections.Generic.List<GrantType>
                {
                    GrantType.refresh_token
                }
            }, null)));
            _tokenStoreStub.Setup(g => g.GetRefreshToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult(new GrantedToken
                {
                    ClientId = "differentId"
                }));

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<IdentityServerException>(() => _getTokenByRefreshTokenGrantTypeAction.Execute(parameter, null, null)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidGrant);
            Assert.True(ex.Message == ErrorDescriptions.TheRefreshTokenCanBeUsedOnlyByTheSameIssuer);
        }

        #endregion

        #region Happy path

        [Fact]
        public async Task When_Requesting_Token_Then_New_One_Is_Generated()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            var parameter = new RefreshTokenGrantTypeParameter();
            var grantedToken = new GrantedToken
            {
                IdTokenPayLoad = new JwsPayload(),
                ClientId = "id"
            };
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>())).Returns(Task.FromResult(new AuthenticationResult(new Core.Common.Models.Client
            {
                ClientId = "id",
                GrantTypes = new System.Collections.Generic.List<GrantType>
                {
                    GrantType.refresh_token
                }
            }, null)));
            _tokenStoreStub.Setup(g => g.GetRefreshToken(It.IsAny<string>()))
                .Returns(Task.FromResult(grantedToken));
            _grantedTokenGeneratorHelperStub.Setup(g => g.GenerateTokenAsync(
                It.IsAny<string>(),
                It.IsAny<string>(), 
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>())).Returns(Task.FromResult(grantedToken));

            // ACT
             await _getTokenByRefreshTokenGrantTypeAction.Execute(parameter, null, null).ConfigureAwait(false);

            // ASSERT
            _tokenStoreStub.Verify(g => g.AddToken(It.IsAny<GrantedToken>()));
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _clientHelperFake = new Mock<IClientHelper>();
            _oauthEventSource = new Mock<IOAuthEventSource>();
            _grantedTokenGeneratorHelperStub = new Mock<IGrantedTokenGeneratorHelper>();
            _tokenStoreStub = new Mock<ITokenStore>();
            _jwtGeneratorStub = new Mock<IJwtGenerator>();
            _authenticateInstructionGeneratorStub = new Mock<IAuthenticateInstructionGenerator>();
            _authenticateClientStub = new Mock<IAuthenticateClient>();
            _getTokenByRefreshTokenGrantTypeAction = new GetTokenByRefreshTokenGrantTypeAction(
                _clientHelperFake.Object,
                _oauthEventSource.Object,
                _grantedTokenGeneratorHelperStub.Object,
                _tokenStoreStub.Object,
                _jwtGeneratorStub.Object,
                _authenticateInstructionGeneratorStub.Object,
                _authenticateClientStub.Object);
        }

        #endregion
    }
}
