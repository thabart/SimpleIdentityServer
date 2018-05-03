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
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Stores;
using SimpleIdentityServer.Core.Validators;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Validators
{
    public class GrantedTokenValidatorFixture
    {
        private Mock<ITokenStore> _grantedTokenRepositoryStub;
        private IGrantedTokenValidator _grantedTokenValidator;

        #region CheckAccessToken

        #region Exceptions

        [Fact]
        public async Task When_Passing_Null_Parameter_To_CheckAccessToken_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _grantedTokenValidator.CheckAccessTokenAsync(null));
        }

        [Fact]
        public async Task When_AccessToken_Doesnt_Exist_Then_False_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _grantedTokenRepositoryStub.Setup(g => g.GetAccessToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));

            // ACT
            var result = await _grantedTokenValidator.CheckAccessTokenAsync("access_token");

            // ASSERT
            Assert.False(result.IsValid);
            Assert.True(result.MessageErrorCode == ErrorCodes.InvalidToken);
            Assert.True(result.MessageErrorDescription == ErrorDescriptions.TheTokenIsNotValid);
        }

        [Fact]
        public async Task When_AccessToken_Is_Expired_Then_False_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var grantedToken = new GrantedToken
            {
                CreateDateTime = DateTime.UtcNow.AddDays(-2),
                ExpiresIn = 200
            };
            _grantedTokenRepositoryStub.Setup(g => g.GetAccessToken(It.IsAny<string>()))
                .Returns(Task.FromResult(grantedToken));

            // ACT
            var result = await _grantedTokenValidator.CheckAccessTokenAsync("access_token");

            // ASSERT
            Assert.False(result.IsValid);
            Assert.True(result.MessageErrorCode == ErrorCodes.InvalidToken);
            Assert.True(result.MessageErrorDescription == ErrorDescriptions.TheTokenIsExpired);
        }

        #endregion

        #region Happy path

        [Fact]
        public async Task When_Checking_Valid_Access_Token_Then_True_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var grantedToken = new GrantedToken
            {
                CreateDateTime = DateTime.UtcNow,
                ExpiresIn = 200000
            };
            _grantedTokenRepositoryStub.Setup(g => g.GetAccessToken(It.IsAny<string>()))
                .Returns(Task.FromResult(grantedToken));

            // ACT
            var result = await _grantedTokenValidator.CheckAccessTokenAsync("access_token");

            // ASSERT
            Assert.True(result.IsValid);
        }

        #endregion

        #endregion

        #region CheckRefreshToken

        #region Exceptions

        [Fact]
        public async Task When_Passing_Null_Parameter_To_CheckRefreshToken_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _grantedTokenValidator.CheckRefreshTokenAsync(null));
        }

        [Fact]
        public async Task When_RefreshToken_Doesnt_Exist_Then_False_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _grantedTokenRepositoryStub.Setup(g => g.GetAccessToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));

            // ACT
            var result = await _grantedTokenValidator.CheckRefreshTokenAsync("refresh_token");

            // ASSERT
            Assert.False(result.IsValid);
            Assert.True(result.MessageErrorCode == ErrorCodes.InvalidToken);
            Assert.True(result.MessageErrorDescription == ErrorDescriptions.TheTokenIsNotValid);
        }

        [Fact]
        public async Task When_RefreshToken_Is_Expired_Then_False_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var grantedToken = new GrantedToken
            {
                CreateDateTime = DateTime.UtcNow.AddDays(-2),
                ExpiresIn = 200
            };
            _grantedTokenRepositoryStub.Setup(g => g.GetRefreshToken(It.IsAny<string>()))
                .Returns(Task.FromResult(grantedToken));

            // ACT
            var result = await _grantedTokenValidator.CheckRefreshTokenAsync("refresh_token");

            // ASSERT
            Assert.False(result.IsValid);
            Assert.True(result.MessageErrorCode == ErrorCodes.InvalidToken);
            Assert.True(result.MessageErrorDescription == ErrorDescriptions.TheTokenIsExpired);
        }

        #endregion

        #region Happy path

        [Fact]
        public async Task When_Checking_Valid_Refresh_Token_Then_True_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var grantedToken = new GrantedToken
            {
                CreateDateTime = DateTime.UtcNow,
                ExpiresIn = 200000
            };
            _grantedTokenRepositoryStub.Setup(g => g.GetRefreshToken(It.IsAny<string>()))
                .Returns(Task.FromResult(grantedToken));

            // ACT
            var result = await _grantedTokenValidator.CheckRefreshTokenAsync("refresh_token");

            // ASSERT
            Assert.True(result.IsValid);
        }

        #endregion

        #endregion

        private void InitializeFakeObjects()
        {
            _grantedTokenRepositoryStub = new Mock<ITokenStore>();
            _grantedTokenValidator = new GrantedTokenValidator(_grantedTokenRepositoryStub.Object);
        }
    }
}
