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
using SimpleIdentityServer.Core.Repositories;
using System;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Helpers
{
    public class GrantedTokenGeneratorHelperFixture
    {
        #region Fields

        private Mock<IConfigurationRepository> _configurationRepository;

        private IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;

        #endregion

        #region Exceptions

        [Fact]
        public void When_Passing_NullOrWhiteSpace_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _grantedTokenGeneratorHelper.GenerateToken(null, null));
            Assert.Throws<ArgumentNullException>(() => _grantedTokenGeneratorHelper.GenerateToken(string.Empty, null));
            Assert.Throws<ArgumentNullException>(() => _grantedTokenGeneratorHelper.GenerateToken("clientid", null));
            Assert.Throws<ArgumentNullException>(() => _grantedTokenGeneratorHelper.GenerateToken("clientid", string.Empty));
        }
        
        #endregion

        #region Happy paths

        [Fact]
        public void When_NoExpiration_Time_Is_Retrieved_Then_3600_Is_Set()
        {
            // ARRANGE
            InitializeFakeObjects();
            _configurationRepository.Setup(c => c.Get(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _grantedTokenGeneratorHelper.GenerateToken("client_id", "scope");

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ExpiresIn == 3600);
        }

        [Fact]
        public void When_ExpirationTime_Is_Set_Then_ExpiresInProperty_Is_Set()
        {
            // ARRANGE
            InitializeFakeObjects();
            var configuration = new Models.Configuration
            {
                Key = "ExpirationTime",
                Value = "3700"
            };
            _configurationRepository.Setup(c => c.Get(It.IsAny<string>()))
                .Returns(configuration);

            // ACT
            var result = _grantedTokenGeneratorHelper.GenerateToken("client_id", "scope");

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ExpiresIn == 3700);
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _configurationRepository = new Mock<IConfigurationRepository>();
            _grantedTokenGeneratorHelper = new GrantedTokenGeneratorHelper(_configurationRepository.Object);
        }

        #endregion
    }
}
