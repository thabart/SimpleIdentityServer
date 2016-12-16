#region copyright
// Copyright 2016 Habart Thierry
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
using SimpleIdentityServer.Client.Configuration;
using SimpleIdentityServer.Client.ResourceSet;
using SimpleIdentityServer.Client.Scope;
using SimpleIdentityServer.Uma.Client.Factory;
using SimpleIdentityServer.Uma.Common.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Host.Tests
{
    public class ScopeFixture : IClassFixture<TestUmaServerFixture>
    {
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IScopeClient _scopeClient;
        private readonly TestUmaServerFixture _server;

        public ScopeFixture(TestUmaServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Adding_Scope_Then_Information_Are_Persisted()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var addResult = await _scopeClient.AddByResolution(new PostScope
            {
                Id = "add_client",
                Name = "add client",
                IconUri = "http://localhost/client.png"
            }, baseUrl + "/.well-known/uma-configuration");
            var scope = await _scopeClient.GetByResolution(addResult.Id, baseUrl + "/.well-known/uma-configuration");

            // ASSERTS
            Assert.NotNull(scope);
            Assert.True(scope.IconUri == "http://localhost/client.png");
            Assert.True(scope.Name == "add client");
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _scopeClient = new ScopeClient(new GetScopeOperation(_httpClientFactoryStub.Object),
                new GetScopesOperation(_httpClientFactoryStub.Object),
                new DeleteScopeOperation(_httpClientFactoryStub.Object),
                new UpdateScopeOperation(_httpClientFactoryStub.Object),
                new AddScopeOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object));
        }
    }
}
