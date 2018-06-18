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
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Helpers
{
    public class GrantedTokenGeneratorHelperFixture
    {
        private Mock<IConfigurationService> _simpleIdentityServerConfiguratorStub;
        private Mock<IJwtGenerator> _jwtGeneratorStub;
        private Mock<IClientHelper> _clientHelperStub;
        private Mock<IClientRepository> _clientRepositoryStub;
        private IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        
        [Fact]
        public async Task When_Passing_NullOrWhiteSpace_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _grantedTokenGeneratorHelper.GenerateTokenAsync(string.Empty, null));
        }

        [Fact]
        public async Task When_Client_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>())).Returns(Task.FromResult((Core.Common.Models.Client)null));

            // ACTS & ASSERTS
            var ex = await Assert.ThrowsAsync<IdentityServerException>(() => _grantedTokenGeneratorHelper.GenerateTokenAsync("invalid_client", null));
            Assert.True(ex.Code == ErrorCodes.InvalidClient);
            Assert.True(ex.Message == ErrorDescriptions.TheClientIdDoesntExist);
        }

        [Fact]
        public async Task When_ExpirationTime_Is_Set_Then_ExpiresInProperty_Is_Set()
        {
            var client = new Core.Common.Models.Client
            {
                ClientId = "client_id"
            };
            // ARRANGE
            InitializeFakeObjects();
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
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
            _jwtGeneratorStub = new Mock<IJwtGenerator>();
            _clientHelperStub = new Mock<IClientHelper>();
            _clientRepositoryStub = new Mock<IClientRepository>();
            _grantedTokenGeneratorHelper = new GrantedTokenGeneratorHelper(
                _simpleIdentityServerConfiguratorStub.Object,
                _jwtGeneratorStub.Object,
                _clientHelperStub.Object,
                _clientRepositoryStub.Object);
        }
    }
}
