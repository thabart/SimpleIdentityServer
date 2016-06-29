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
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using System;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Helpers
{
    public class GrantedTokenHelperFixture
    {
        #region Fields

        private Mock<IGrantedTokenRepository> _grantedTokenRepositoryStub;

        private Mock<IGrantedTokenValidator> _grantedTokenValidatorStub;

        private IGrantedTokenHelper _grantedTokenHelper;

        #endregion

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _grantedTokenHelper.GetValidGrantedToken(null, null, null, null));
            Assert.Throws<ArgumentNullException>(() => _grantedTokenHelper.GetValidGrantedToken("scopes", null, null, null));
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_Valid_Token_Doesnt_Exist_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _grantedTokenRepositoryStub.Setup(g => g.GetToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<JwsPayload>(), It.IsAny<JwsPayload>()))
                .Returns((GrantedToken)null);

            // ACT
            var result = _grantedTokenHelper.GetValidGrantedToken("scopes", "client_id", null, null);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_GrantedToken_Is_Expired_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string messageErrorCode;
            string messageErrorDescription;
            _grantedTokenRepositoryStub.Setup(g => g.GetToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<JwsPayload>(), It.IsAny<JwsPayload>()))
                .Returns(new GrantedToken());

            _grantedTokenValidatorStub.Setup(g => g.CheckAccessToken(It.IsAny<string>(), out messageErrorCode, out messageErrorDescription))
                .Returns(false);

            // ACT
            var result = _grantedTokenHelper.GetValidGrantedToken("scopes", "client_id", null, null);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Token_Exists_Then_GrantedToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            string messageErrorCode;
            string messageErrorDescription;
            _grantedTokenRepositoryStub.Setup(g => g.GetToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<JwsPayload>(), It.IsAny<JwsPayload>()))
                .Returns(new GrantedToken());

            _grantedTokenValidatorStub.Setup(g => g.CheckAccessToken(It.IsAny<string>(), out messageErrorCode, out messageErrorDescription))
                .Returns(true);

            // ACT
            var result = _grantedTokenHelper.GetValidGrantedToken("scopes", "client_id", null, null);

            // ASSERT
            Assert.NotNull(result);
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _grantedTokenRepositoryStub = new Mock<IGrantedTokenRepository>();
            _grantedTokenValidatorStub = new Mock<IGrantedTokenValidator>();
            _grantedTokenHelper = new GrantedTokenHelper(
                _grantedTokenRepositoryStub.Object,
                _grantedTokenValidatorStub.Object);
        }

        #endregion
    }
}
