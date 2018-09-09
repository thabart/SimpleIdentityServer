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
using SimpleIdentityServer.Client.Permission;
using SimpleIdentityServer.Client.Policy;
using SimpleIdentityServer.Client.ResourceSet;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Uma.Client.Policy;
using SimpleIdentityServer.Uma.Client.ResourceSet;
using SimpleIdentityServer.Uma.Common.DTOs;
using SimpleIdentityServer.Uma.Host.Tests.MiddleWares;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Host.Tests
{
    public class PermissionFixture : IClassFixture<TestUmaServerFixture>
    {
        const string baseUrl = "http://localhost:5000";
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IPolicyClient _policyClient;
        private IResourceSetClient _resourceSetClient;
        private IPermissionClient _permissionClient;
        private readonly TestUmaServerFixture _server;

        public PermissionFixture(TestUmaServerFixture server)
        {
            _server = server;
        }


        #region Errors

        [Fact]
        public async Task When_Client_Is_Not_Authenticated_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
                {
                    Name = "picture",
                    Scopes = new List<string>
                    {
                        "read"
                    }
                }, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ACT
            UserStore.Instance().ClientId = null;
            var ticket = await _permissionClient.AddByResolution(new PostPermission
                {
                    ResourceSetId = resource.Content.Id,
                    Scopes = new List<string>
                    {
                        "read"
                    }
                }, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);
            UserStore.Instance().ClientId = "client";

            // ASSERTS
            Assert.True(ticket.ContainsError);
            Assert.Equal("invalid_request", ticket.Error.Error);
            Assert.Equal("the client_id cannot be extracted", ticket.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_ResourceSetId_Is_Null_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var ticket = await _permissionClient.AddByResolution(new PostPermission
                {
                    ResourceSetId = string.Empty
                }, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERTS
            Assert.True(ticket.ContainsError);
            Assert.Equal("invalid_request", ticket.Error.Error);
            Assert.Equal("the parameter resource_set_id needs to be specified", ticket.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Scopes_Is_Null_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var ticket = await _permissionClient.AddByResolution(new PostPermission
                {
                    ResourceSetId = "resource"
                }, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERTS
            Assert.True(ticket.ContainsError);
            Assert.Equal("invalid_request", ticket.Error.Error);
            Assert.Equal("the parameter scopes needs to be specified", ticket.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Resource_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var ticket = await _permissionClient.AddByResolution(new PostPermission
                {
                    ResourceSetId = "resource",
                    Scopes = new List<string>
                    {
                        "scope"
                    }
                }, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERTS
            Assert.True(ticket.ContainsError);
            Assert.Equal("invalid_resource_set_id", ticket.Error.Error);
            Assert.Equal("resource set resource doesn't exist", ticket.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Scopes_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
                {
                    Name = "picture",
                    Scopes = new List<string>
                    {
                        "read"
                    }
                }, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ACT
            var ticket = await _permissionClient.AddByResolution(new PostPermission
                {
                    ResourceSetId = resource.Content.Id,
                    Scopes = new List<string>
                    {
                        "scopescopescope"
                    }
                }, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERTS
            Assert.True(ticket.ContainsError);
            Assert.Equal("invalid_scope", ticket.Error.Error);
            Assert.Equal("one or more scopes are not valid", ticket.Error.ErrorDescription);
        }

        #endregion

        #region Happy paths

        [Fact]
        public async Task When_Adding_Permission_Then_TicketId_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
                {
                    Name = "picture",
                    Scopes = new List<string>
                    {
                        "read"
                    }
                }, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ACT
            var ticket = await _permissionClient.AddByResolution(new PostPermission
                {
                    ResourceSetId = resource.Content.Id,
                    Scopes = new List<string>
                    {
                        "read"
                    }
                }, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(ticket);
            Assert.NotEmpty(ticket.Content.TicketId);
        }

        [Fact]
        public async Task When_Adding_Permissions_Then_TicketIds_Is_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
                {
                    Name = "picture",
                    Scopes = new List<string>
                    {
                        "read"
                    }
                }, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);
            var permissions = new List<PostPermission>
            {
                new PostPermission
                {
                    ResourceSetId = resource.Content.Id,
                    Scopes = new List<string>
                    {
                        "read"
                    }
                },
                new PostPermission
                {
                    ResourceSetId = resource.Content.Id,
                    Scopes = new List<string>
                    {
                        "read"
                    }
                }
            };

            // ACT
            var ticket = await _permissionClient.AddByResolution(permissions, baseUrl + "/.well-known/uma2-configuration", "header").ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(ticket);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _policyClient = new PolicyClient(new AddPolicyOperation(_httpClientFactoryStub.Object),
                new GetPolicyOperation(_httpClientFactoryStub.Object),
                new DeletePolicyOperation(_httpClientFactoryStub.Object),
                new GetPoliciesOperation(_httpClientFactoryStub.Object),
                new AddResourceToPolicyOperation(_httpClientFactoryStub.Object),
                new DeleteResourceFromPolicyOperation(_httpClientFactoryStub.Object),
                new UpdatePolicyOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object),
				new SearchPoliciesOperation(_httpClientFactoryStub.Object));
            _resourceSetClient = new ResourceSetClient(new AddResourceSetOperation(_httpClientFactoryStub.Object),
                new DeleteResourceSetOperation(_httpClientFactoryStub.Object),
                new GetResourcesOperation(_httpClientFactoryStub.Object),
                new GetResourceOperation(_httpClientFactoryStub.Object),
                new UpdateResourceOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object),
				new SearchResourcesOperation(_httpClientFactoryStub.Object));
            _permissionClient = new PermissionClient(
                new AddPermissionsOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object));
        }
    }
}
