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
using SimpleIdentityServer.Client.Selectors;
using System;
using Xunit;

namespace SimpleIdentityServer.Client.Unit.Tests.Selectors
{
    public class ClientAuthSelectorFixture
    {
        private Mock<ITokenClientFactory> _tokenClientFactoryStub;
        private IClientAuthSelector _clientAuthSelector;

        [Fact]
        public void When_Passing_Null_Parameters_To_ClientSecretBasic_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _clientAuthSelector.UseClientSecretBasicAuth(null, null));
            Assert.Throws<ArgumentNullException>(() => _clientAuthSelector.UseClientSecretBasicAuth("client_id", null));
        }

        [Fact]
        public void When_Passing_Null_Parameters_To_ClientSecretPost_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _clientAuthSelector.UseClientSecretPostAuth(null, null));
            Assert.Throws<ArgumentNullException>(() => _clientAuthSelector.UseClientSecretPostAuth("client_id", null));
        }

        [Fact]
        public void When_Passing_Null_Parameters_To_ClientSecretJwtAuth_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _clientAuthSelector.UseClientSecretJwtAuth(null, null));
            Assert.Throws<ArgumentNullException>(() => _clientAuthSelector.UseClientSecretJwtAuth("client_id", null));
        }

        private void InitializeFakeObjects()
        {
            _tokenClientFactoryStub = new Mock<ITokenClientFactory>();
            _clientAuthSelector = new ClientAuthSelector(_tokenClientFactoryStub.Object);
        }
    }
}
