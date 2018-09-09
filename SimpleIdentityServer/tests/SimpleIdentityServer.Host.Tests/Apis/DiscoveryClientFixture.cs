﻿#region copyright
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
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Common.Client.Factories;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests
{
    public class DiscoveryClientFixture : IClassFixture<TestOauthServerFixture>
    {
        private readonly TestOauthServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IDiscoveryClient _discoveryClient;

        public DiscoveryClientFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Retrieving_DiscoveryInformation_Then_No_Exception_Is_Thrown()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var discovery = await _discoveryClient.GetDiscoveryInformationAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(discovery);
            Assert.True(discovery.ScimEndpoint == FakeStartup.ScimEndPoint);
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _discoveryClient = new DiscoveryClient(new GetDiscoveryOperation(_httpClientFactoryStub.Object));
        }
    }
}
