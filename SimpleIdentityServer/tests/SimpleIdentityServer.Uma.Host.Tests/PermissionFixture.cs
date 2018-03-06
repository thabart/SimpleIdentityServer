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
using SimpleIdentityServer.Uma.Client.Factory;
using SimpleIdentityServer.Uma.Common.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Host.Tests
{
    public class PermissionFixture : IClassFixture<TestUmaServerFixture>
    {
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IPolicyClient _policyClient;
        private IResourceSetClient _resourceSetClient;
        private IPermissionClient _permissionClient;
        private readonly TestUmaServerFixture _server;

        public PermissionFixture(TestUmaServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Adding_Permission_Then_TicketId_Is_Returned()
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
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ACT
            var ticket = await _permissionClient.AddByResolution(new PostPermission
            {
                ResourceSetId = resource.Id,
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(ticket);
            Assert.NotEmpty(ticket.TicketId);
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
            }, baseUrl + "/.well-known/uma2-configuration", "header");
            var permissions = new List<PostPermission>
            {
                new PostPermission
                {
                    ResourceSetId = resource.Id,
                    Scopes = new List<string>
                    {
                        "read"
                    }
                },
                new PostPermission
                {
                    ResourceSetId = resource.Id,
                    Scopes = new List<string>
                    {
                        "read"
                    }
                }
            };

            // ACT
            var ticket = await _permissionClient.AddByResolution(permissions, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(ticket);
        }

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
                new GetConfigurationOperation(_httpClientFactoryStub.Object));
            _resourceSetClient = new ResourceSetClient(new AddResourceSetOperation(_httpClientFactoryStub.Object),
                new DeleteResourceSetOperation(_httpClientFactoryStub.Object),
                new GetResourcesOperation(_httpClientFactoryStub.Object),
                new GetResourceOperation(_httpClientFactoryStub.Object),
                new UpdateResourceOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object));
            _permissionClient = new PermissionClient(
                new AddPermissionsOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object));
        }
    }
}
