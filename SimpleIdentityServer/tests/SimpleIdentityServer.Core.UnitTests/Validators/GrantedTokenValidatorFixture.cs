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
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using System;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Validators
{
    public class GrantedTokenValidatorFixture
    {
        private Mock<IGrantedTokenRepository> _grantedTokenRepositoryStub;

        private IGrantedTokenValidator _grantedTokenValidator;

        #region CheckAccessToken

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_To_CheckAccessToken_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            string messageErrorCode;
            string messageErrorDescription;

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _grantedTokenValidator.CheckAccessToken(null, out messageErrorCode, out messageErrorDescription));
        }

        [Fact]
        public void When_AccessToken_Doesnt_Exist_Then_False_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string messageErrorCode;
            string messageErrorDescription;
            _grantedTokenRepositoryStub.Setup(g => g.GetToken(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _grantedTokenValidator.CheckAccessToken("access_token", out messageErrorCode, out messageErrorDescription);

            // ASSERT
            Assert.False(result);
            Assert.True(messageErrorCode == ErrorCodes.InvalidToken);
            Assert.True(messageErrorDescription == ErrorDescriptions.TheTokenIsNotValid);
        }

        [Fact]
        public void When_AccessToken_Is_Expired_Then_False_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string messageErrorCode;
            string messageErrorDescription;
            var grantedToken = new GrantedToken
            {
                CreateDateTime = DateTime.UtcNow.AddDays(-2),
                ExpiresIn = 200
            };
            _grantedTokenRepositoryStub.Setup(g => g.GetToken(It.IsAny<string>()))
                .Returns(grantedToken);

            // ACT
            var result = _grantedTokenValidator.CheckAccessToken("access_token", out messageErrorCode, out messageErrorDescription);

            // ASSERT
            Assert.False(result);
            Assert.True(messageErrorCode == ErrorCodes.InvalidToken);
            Assert.True(messageErrorDescription == ErrorDescriptions.TheTokenIsExpired);
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_Checking_Valid_Access_Token_Then_True_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string messageErrorCode;
            string messageErrorDescription;
            var grantedToken = new GrantedToken
            {
                CreateDateTime = DateTime.UtcNow,
                ExpiresIn = 200000
            };
            _grantedTokenRepositoryStub.Setup(g => g.GetToken(It.IsAny<string>()))
                .Returns(grantedToken);

            // ACT
            var result = _grantedTokenValidator.CheckAccessToken("access_token", out messageErrorCode, out messageErrorDescription);

            // ASSERT
            Assert.True(result);
        }

        #endregion

        #endregion

        #region CheckRefreshToken

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_To_CheckRefreshToken_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            string messageErrorCode;
            string messageErrorDescription;

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _grantedTokenValidator.CheckRefreshToken(null, out messageErrorCode, out messageErrorDescription));
        }

        [Fact]
        public void When_RefreshToken_Doesnt_Exist_Then_False_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string messageErrorCode;
            string messageErrorDescription;
            _grantedTokenRepositoryStub.Setup(g => g.GetToken(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _grantedTokenValidator.CheckRefreshToken("refresh_token", out messageErrorCode, out messageErrorDescription);

            // ASSERT
            Assert.False(result);
            Assert.True(messageErrorCode == ErrorCodes.InvalidToken);
            Assert.True(messageErrorDescription == ErrorDescriptions.TheTokenIsNotValid);
        }

        [Fact]
        public void When_RefreshToken_Is_Expired_Then_False_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string messageErrorCode;
            string messageErrorDescription;
            var grantedToken = new GrantedToken
            {
                CreateDateTime = DateTime.UtcNow.AddDays(-2),
                ExpiresIn = 200
            };
            _grantedTokenRepositoryStub.Setup(g => g.GetTokenByRefreshToken(It.IsAny<string>()))
                .Returns(grantedToken);

            // ACT
            var result = _grantedTokenValidator.CheckRefreshToken("refresh_token", out messageErrorCode, out messageErrorDescription);

            // ASSERT
            Assert.False(result);
            Assert.True(messageErrorCode == ErrorCodes.InvalidToken);
            Assert.True(messageErrorDescription == ErrorDescriptions.TheTokenIsExpired);
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_Checking_Valid_Refresh_Token_Then_True_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string messageErrorCode;
            string messageErrorDescription;
            var grantedToken = new GrantedToken
            {
                CreateDateTime = DateTime.UtcNow,
                ExpiresIn = 200000
            };
            _grantedTokenRepositoryStub.Setup(g => g.GetTokenByRefreshToken(It.IsAny<string>()))
                .Returns(grantedToken);

            // ACT
            var result = _grantedTokenValidator.CheckRefreshToken("refresh_token", out messageErrorCode, out messageErrorDescription);

            // ASSERT
            Assert.True(result);
        }

        #endregion

        #endregion

        private void InitializeFakeObjects()
        {
            _grantedTokenRepositoryStub = new Mock<IGrantedTokenRepository>();
            _grantedTokenValidator = new GrantedTokenValidator(_grantedTokenRepositoryStub.Object);
        }
    }
}
