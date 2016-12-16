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

        [Fact]
        public async Task When_Removing_Scope_Then_It_Cannot_Be_Retrieved()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var addResult = await _scopeClient.AddByResolution(new PostScope
            {
                Id = "remove_client",
                Name = "remove client",
                IconUri = "http://localhost/client.png"
            }, baseUrl + "/.well-known/uma-configuration");
            var isRemoved = await _scopeClient.DeleteByResolution(addResult.Id, baseUrl + "/.well-known/uma-configuration");
            var ex = await Assert.ThrowsAsync<HttpRequestException>(() => _scopeClient.GetByResolution(addResult.Id, baseUrl + "/.well-known/uma-configuration"));

            // ASSERTS
            Assert.True(isRemoved);
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task When_Retrieving_Scopes_Then_Identifiers_Are_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var addResult = await _scopeClient.AddByResolution(new PostScope
            {
                Id = "other_client",
                Name = "other client",
                IconUri = "http://localhost/client.png"
            }, baseUrl + "/.well-known/uma-configuration");
            var identifiers = await _scopeClient.GetAllByResolution(baseUrl + "/.well-known/uma-configuration");

            // ASSERTS
            Assert.NotNull(identifiers);
            Assert.True(identifiers.Any(i => i == addResult.Id));
        }

        [Fact]
        public async Task When_Updating_Scope_Then_Information_Are_Persisted()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var addResult = await _scopeClient.AddByResolution(new PostScope
            {
                Id = "add_customer",
                Name = "add customer",
                IconUri = "http://localhost/customer.png"
            }, baseUrl + "/.well-known/uma-configuration");
            var updateResult = await _scopeClient.UpdateByResolution(new PutScope
            {
                Id = addResult.Id,
                Name = "modified customer",
                IconUri = "http://localhost/modifier_customer.png"
            }, baseUrl + "/.well-known/uma-configuration");
            var information = await _scopeClient.GetByResolution(addResult.Id, baseUrl + "/.well-known/uma-configuration");

            // ASSERTS
            Assert.NotNull(information);
            Assert.True(information.IconUri == "http://localhost/modifier_customer.png");
            Assert.True(information.Name == "modified customer");
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
