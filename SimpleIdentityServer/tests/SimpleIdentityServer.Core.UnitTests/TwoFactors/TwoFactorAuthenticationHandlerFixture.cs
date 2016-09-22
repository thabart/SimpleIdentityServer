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
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.TwoFactors;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.TwoFactors
{
    public class TwoFactorAuthenticationHandlerFixture
    {
        private Mock<ISimpleIdServerConfigurationClientFactory> _simpleIdServerConfigurationClientFactoryStub;

        private Mock<ISimpleIdentityServerConfigurator> _simpleIdentityServerConfiguratorStub;

        private Mock<IIdentityServerClientFactory> _identityServerClientFactoryStub;

        private ITwoFactorAuthenticationHandler _twoFactorAuthenticationHandler;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.ThrowsAsync<ArgumentNullException>(() => _twoFactorAuthenticationHandler.SendCode(null, 0, null));
            Assert.ThrowsAsync<ArgumentNullException>(() => _twoFactorAuthenticationHandler.SendCode(string.Empty, 0, null));
            Assert.ThrowsAsync<ArgumentNullException>(() => _twoFactorAuthenticationHandler.SendCode("code", 0, null));
        }

        [Fact]
        public async Task When_Passing_NotSupported_ProviderType_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _twoFactorAuthenticationHandler.SendCode("code", 3, new Models.ResourceOwner()));
            Assert.NotNull(exception);
            Assert.True(exception.Message == $"the service 3 doesn't exist");
        }

        private void InitializeFakeObjects()
        {
            _simpleIdServerConfigurationClientFactoryStub = new Mock<ISimpleIdServerConfigurationClientFactory>();
            _simpleIdentityServerConfiguratorStub = new Mock<ISimpleIdentityServerConfigurator>();
            _identityServerClientFactoryStub = new Mock<IIdentityServerClientFactory>();
            _twoFactorAuthenticationHandler = new TwoFactorAuthenticationHandler(
                _simpleIdServerConfigurationClientFactoryStub.Object,
                _identityServerClientFactoryStub.Object,
                _simpleIdentityServerConfiguratorStub.Object);
        }
    }
}
