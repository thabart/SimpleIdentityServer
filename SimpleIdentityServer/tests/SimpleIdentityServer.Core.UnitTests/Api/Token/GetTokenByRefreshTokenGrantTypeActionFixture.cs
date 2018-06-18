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
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Store;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Token
{
    public sealed class GetTokenByRefreshTokenGrantTypeActionFixture
    {
        private Mock<IClientHelper> _clientHelperFake;
        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceStub;
        private Mock<IGrantedTokenGeneratorHelper> _grantedTokenGeneratorHelperStub;
        private Mock<ITokenStore> _tokenStoreStub;
        private Mock<IJwtGenerator> _jwtGeneratorStub;
        private IGetTokenByRefreshTokenGrantTypeAction _getTokenByRefreshTokenGrantTypeAction;

        #region Exceptions

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getTokenByRefreshTokenGrantTypeAction.Execute(null));
        }

        [Fact]
        public async Task When_Passing_Invalid_Refresh_Token_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RefreshTokenGrantTypeParameter();
            _tokenStoreStub.Setup(g => g.GetRefreshToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<IdentityServerException>(() => _getTokenByRefreshTokenGrantTypeAction.Execute(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidGrant);
            Assert.True(ex.Message == ErrorDescriptions.TheRefreshTokenIsNotValid);
        }

        #endregion

        #region Happy path

        [Fact]
        public async Task When_Requesting_Token_Then_New_One_Is_Generated()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RefreshTokenGrantTypeParameter();
            var grantedToken = new GrantedToken
            {
                IdTokenPayLoad = new JwsPayload()
            };
            _tokenStoreStub.Setup(g => g.GetRefreshToken(It.IsAny<string>()))
                .Returns(Task.FromResult(grantedToken));
            _grantedTokenGeneratorHelperStub.Setup(g => g.GenerateTokenAsync(
                It.IsAny<string>(),
                It.IsAny<string>(), 
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>())).Returns(Task.FromResult(grantedToken));

            // ACT
             await _getTokenByRefreshTokenGrantTypeAction.Execute(parameter);

            // ASSERT
            _tokenStoreStub.Verify(g => g.AddToken(It.IsAny<GrantedToken>()));
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _clientHelperFake = new Mock<IClientHelper>();
            _simpleIdentityServerEventSourceStub = new Mock<ISimpleIdentityServerEventSource>();
            _grantedTokenGeneratorHelperStub = new Mock<IGrantedTokenGeneratorHelper>();
            _tokenStoreStub = new Mock<ITokenStore>();
            _jwtGeneratorStub = new Mock<IJwtGenerator>();
            _getTokenByRefreshTokenGrantTypeAction = new GetTokenByRefreshTokenGrantTypeAction(
                _clientHelperFake.Object,
                _simpleIdentityServerEventSourceStub.Object,
                _grantedTokenGeneratorHelperStub.Object,
                _tokenStoreStub.Object,
                _jwtGeneratorStub.Object);
        }

        #endregion
    }
}
