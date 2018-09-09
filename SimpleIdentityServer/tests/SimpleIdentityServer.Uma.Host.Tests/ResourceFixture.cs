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

using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleIdentityServer.Client.Configuration;
using SimpleIdentityServer.Client.ResourceSet;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Uma.Client.ResourceSet;
using SimpleIdentityServer.Uma.Common.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Host.Tests
{
    public class ResourceFixture : IClassFixture<TestUmaServerFixture>
    {
        const string baseUrl = "http://localhost:5000";
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IResourceSetClient _resourceSetClient;
        private readonly TestUmaServerFixture _server;

        public ResourceFixture(TestUmaServerFixture server)
        {
            _server = server;
        }

        #region Errors

        #region Add

        [Fact]
        public async Task When_Add_Resource_And_No_Name_Is_Specified_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
                {
                    Name = string.Empty
                },
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("invalid_request", resource.Error.Error);
            Assert.Equal("the parameter name needs to be specified", resource.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Resource_And_No_Scopes_Is_Specified_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
                {
                    Name = "name"
                },
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("invalid_request", resource.Error.Error);
            Assert.Equal("the parameter scopes needs to be specified", resource.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Resource_And_No_Invalid_IconUri_Is_Specified_Then_Error_Is_Returned()
        {
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
                    },
                    IconUri = "invalid"
                },
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("invalid_request", resource.Error.Error);
            Assert.Equal("the url invalid is not well formed", resource.Error.ErrorDescription);
        }

        #endregion

        #region Get

        [Fact]
        public async Task When_Get_Unknown_Resource_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.GetByResolution("unknown",
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("not_found", resource.Error.Error);
            Assert.Equal("resource cannot be found", resource.Error.ErrorDescription);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task When_Delete_Unknown_Resource_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.DeleteByResolution("unknown",
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("not_found", resource.Error.Error);
            Assert.Equal("resource cannot be found", resource.Error.ErrorDescription);
        }

        #endregion

        #region Update

        [Fact]
        public async Task When_Update_Resource_And_No_Id_Is_Specified_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.UpdateByResolution(new PutResourceSet(), baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("invalid_request", resource.Error.Error);
            Assert.Equal("the parameter id needs to be specified", resource.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Resource_And_No_Name_Is_Specified_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.UpdateByResolution(new PutResourceSet
                {
                    Id = "invalid",
                    Name = string.Empty
                },
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("invalid_request", resource.Error.Error);
            Assert.Equal("the parameter name needs to be specified", resource.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Resource_And_No_Scopes_Is_Specified_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.UpdateByResolution(new PutResourceSet
                {
                    Id = "invalid",
                    Name = "name"
                },
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("invalid_request", resource.Error.Error);
            Assert.Equal("the parameter scopes needs to be specified", resource.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Resource_And_No_Invalid_IconUri_Is_Specified_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.UpdateByResolution(new PutResourceSet
                {
                    Id = "invalid",
                    Name = "name",
                    Scopes = new List<string>
                    {
                        "scope"
                    },
                    IconUri = "invalid"
                },
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("invalid_request", resource.Error.Error);
            Assert.Equal("the url invalid is not well formed", resource.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Unknown_Resource_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.UpdateByResolution(new PutResourceSet
                {
                    Id = "invalid",
                    Name = "name",
                    Scopes = new List<string>
                    {
                        "scope"
                    }
                },
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(resource);
            Assert.True(resource.ContainsError);
            Assert.Equal("not_found", resource.Error.Error);
            Assert.Equal("resource cannot be found", resource.Error.ErrorDescription);
        }

        #endregion

        #endregion

        #region Happy path

        #region Get all

        [Fact]
        public async Task When_Getting_Resources_Then_Identifiers_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resources = await _resourceSetClient.GetAllByResolution(
                baseUrl + "/.well-known/uma2-configuration", "token").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(resources.Content);
            Assert.True(resources.Content.Any());
        }

        #endregion

        #region Get

        [Fact]
        public async Task When_Getting_ResourceInformation_Then_Dto_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resources = await _resourceSetClient.GetAllByResolution(
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);
            var resource = await _resourceSetClient.GetByResolution(resources.Content.First(),
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(resource);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task When_Deleting_ResourceInformation_Then_It_Doesnt_Exist()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resources = await _resourceSetClient.GetAllByResolution(
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);
            var resource = await _resourceSetClient.DeleteByResolution(resources.Content.First(),
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);
            var information = await _resourceSetClient.GetByResolution(resources.Content.First(),
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.False(resource.ContainsError);
            Assert.True(information.ContainsError);
            Assert.NotNull(information);
        }

        #endregion

        #region Add

        [Fact]
        public async Task When_Adding_Resource_Then_Information_Can_Be_Retrieved()
        {
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
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(resource);
        }

        #endregion

        #region Search

        [Fact]
        public async Task When_Search_Resources_Then_List_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resource = await _resourceSetClient.ResolveSearch(baseUrl + "/.well-known/uma2-configuration", new SearchResourceSet
                {
                    StartIndex = 0,
                    TotalResults = 100
                },
                "header").ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(resource);
            Assert.False(resource.ContainsError);
            Assert.True(resource.Content.Content.Any());
        }

        #endregion

        #region Update

        [Fact]
        public async Task When_Updating_Resource_Then_Changes_Are_Persisted()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
                {
                    Name = "name",
                    Scopes = new List<string>
                    {
                        "scope"
                    }
                },
                baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ACT
            var updateResult = await _resourceSetClient.UpdateByResolution(new PutResourceSet
                {
                    Id = resource.Content.Id,
                    Name = "name2",
                    Type = "type",
                    Scopes = new List<string>
                    {
                        "scope2"
                    }
                }, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);
            var information = await _resourceSetClient.GetByResolution(updateResult.Content.Id, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(information);
            Assert.True(information.Content.Name == "name2");
            Assert.True(information.Content.Type == "type");
            Assert.True(information.Content.Scopes.Count() == 1 && information.Content.Scopes.First() == "scope2");
        }

        #endregion

        #endregion

        private void InitializeFakeObjects()
        {
            var services = new ServiceCollection();
            services.AddSimpleIdentityServerJwt();
            var provider = services.BuildServiceProvider();
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _resourceSetClient = new ResourceSetClient(new AddResourceSetOperation(_httpClientFactoryStub.Object),
                new DeleteResourceSetOperation(_httpClientFactoryStub.Object),
                new GetResourcesOperation(_httpClientFactoryStub.Object),
                new GetResourceOperation(_httpClientFactoryStub.Object),
                new UpdateResourceOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object),
				new SearchResourcesOperation(_httpClientFactoryStub.Object));
        }
    }
}
