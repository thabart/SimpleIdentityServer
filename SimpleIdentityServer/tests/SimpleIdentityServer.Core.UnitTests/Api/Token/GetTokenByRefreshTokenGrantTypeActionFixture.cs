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
using Moq;
using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using Xunit;
using SimpleIdentityServer.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.UnitTests.Api.Token
{
    public sealed class GetTokenByRefreshTokenGrantTypeActionFixture
    {
        private Mock<IGrantedTokenRepository> _grantedTokenRepositoryFake;
        private Mock<IClientHelper> _clientHelperFake;
        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceStub;
        private Mock<IGrantedTokenGeneratorHelper> _grantedTokenGeneratorHelperStub;
        private IGetTokenByRefreshTokenGrantTypeAction _getTokenByRefreshTokenGrantTypeAction;

        #region Exceptions

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getTokenByRefreshTokenGrantTypeAction.Execute(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Invalid_Refresh_Token_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RefreshTokenGrantTypeParameter();
            _grantedTokenRepositoryFake.Setup(g => g.GetTokenByRefreshTokenAsync(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<IdentityServerException>(() => _getTokenByRefreshTokenGrantTypeAction.Execute(parameter)).ConfigureAwait(false);
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
            _grantedTokenRepositoryFake.Setup(g => g.GetTokenByRefreshTokenAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(grantedToken));
            _grantedTokenGeneratorHelperStub.Setup(g => g.GenerateTokenAsync(
                It.IsAny<string>(),
                It.IsAny<string>(), 
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>())).Returns(Task.FromResult(grantedToken));

            // ACT
             await _getTokenByRefreshTokenGrantTypeAction.Execute(parameter).ConfigureAwait(false);

            // ASSERT
            _grantedTokenRepositoryFake.Verify(g => g.InsertAsync(It.IsAny<GrantedToken>()));
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _grantedTokenRepositoryFake = new Mock<IGrantedTokenRepository>();
            _clientHelperFake = new Mock<IClientHelper>();
            _simpleIdentityServerEventSourceStub = new Mock<ISimpleIdentityServerEventSource>();
            _grantedTokenGeneratorHelperStub = new Mock<IGrantedTokenGeneratorHelper>();
            _getTokenByRefreshTokenGrantTypeAction = new GetTokenByRefreshTokenGrantTypeAction(
                _grantedTokenRepositoryFake.Object,
                _clientHelperFake.Object,
                _simpleIdentityServerEventSourceStub.Object,
                _grantedTokenGeneratorHelperStub.Object);
        }

        #endregion
    }
}
