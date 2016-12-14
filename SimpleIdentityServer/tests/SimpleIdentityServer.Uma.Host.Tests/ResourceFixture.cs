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
using SimpleIdentityServer.Uma.Client.Factory;
using SimpleIdentityServer.Uma.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Host.Tests
{
    public class ResourceFixture : IClassFixture<TestUmaServerFixture>
    {
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IResourceSetClient _resourceSetClient;
        private readonly TestUmaServerFixture _server;

        public ResourceFixture(TestUmaServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Getting_Resources_Then_Identifiers_Are_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resources = await _resourceSetClient.GetAllByResolution(
                baseUrl + "/.well-known/uma-configuration", "header");

            // ASSERT
            Assert.NotNull(resources);
            Assert.True(resources.Any());
        }

        [Fact]
        public async Task When_Getting_ResourceInformation_Then_Dto_Is_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resources = await _resourceSetClient.GetAllByResolution(
                baseUrl + "/.well-known/uma-configuration", "header");
            var resource = await _resourceSetClient.GetByResolution(resources.First(),
                baseUrl + "/.well-known/uma-configuration", "header");

            // ASSERT
            Assert.NotNull(resource);
        }

        [Fact]
        public async Task When_Deleting_ResourceInformation_Then_It_Doesnt_Exist()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resources = await _resourceSetClient.GetAllByResolution(
                baseUrl + "/.well-known/uma-configuration", "header");
            var resource = await _resourceSetClient.DeleteByResolution(resources.First(),
                baseUrl + "/.well-known/uma-configuration", "header");
            var information = await Assert.ThrowsAsync<HttpRequestException>(() => _resourceSetClient.GetByResolution(resources.First(),
                baseUrl + "/.well-known/uma-configuration", "header"));

            // ASSERT
            Assert.True(resource);
            Assert.NotNull(information);
        }

        [Fact]
        public async Task When_Adding_Resource_Then_Information_Can_Be_Retrieved()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "name",
                Scopes = new List<string>
                {
                    "scope"
                }
            },
            baseUrl + "/.well-known/uma-configuration", "header");

            // ASSERT
            Assert.NotNull(resource);
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _resourceSetClient = new ResourceSetClient(new AddResourceSetOperation(_httpClientFactoryStub.Object),
                new DeleteResourceSetOperation(_httpClientFactoryStub.Object),
                new GetResourcesOperation(_httpClientFactoryStub.Object),
                new GetResourceOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object));
        }
    }
}
