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
using SimpleIdentityServer.Core.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Helpers
{
    public class GrantedTokenGeneratorHelperFixture
    {
        private Mock<IConfigurationService> _simpleIdentityServerConfiguratorStub;
        private IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        
        [Fact]
        public async Task When_Passing_NullOrWhiteSpace_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _grantedTokenGeneratorHelper.GenerateTokenAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _grantedTokenGeneratorHelper.GenerateTokenAsync(string.Empty, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _grantedTokenGeneratorHelper.GenerateTokenAsync("clientid", null));
        }

        [Fact]
        public async Task When_ExpirationTime_Is_Set_Then_ExpiresInProperty_Is_Set()
        {
            // ARRANGE
            InitializeFakeObjects();
            _simpleIdentityServerConfiguratorStub.Setup(c => c.GetTokenValidityPeriodInSecondsAsync())
                .Returns(Task.FromResult((double)3700));

            // ACT
            var result = await _grantedTokenGeneratorHelper.GenerateTokenAsync("client_id", "scope");

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ExpiresIn == 3700);
        }

        private void InitializeFakeObjects()
        {
            _simpleIdentityServerConfiguratorStub = new Mock<IConfigurationService>();
            _grantedTokenGeneratorHelper = new GrantedTokenGeneratorHelper(_simpleIdentityServerConfiguratorStub.Object);
        }
    }
}
