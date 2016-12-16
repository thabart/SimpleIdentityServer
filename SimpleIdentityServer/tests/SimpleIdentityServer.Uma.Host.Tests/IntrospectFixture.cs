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
using SimpleIdentityServer.Client.Authorization;
using SimpleIdentityServer.Client.Configuration;
using SimpleIdentityServer.Client.Introspection;
using SimpleIdentityServer.Client.Permission;
using SimpleIdentityServer.Client.Policy;
using SimpleIdentityServer.Client.ResourceSet;
using SimpleIdentityServer.Uma.Client.Factory;
using SimpleIdentityServer.Uma.Common.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Host.Tests
{
    public class IntrospectFixture : IClassFixture<TestUmaServerFixture>
    {
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IPolicyClient _policyClient;
        private IResourceSetClient _resourceSetClient;
        private IPermissionClient _permissionClient;
        private IAuthorizationClient _authorizationClient;
        private IIntrospectionClient _introspectionClient;
        private readonly TestUmaServerFixture _server;

        public IntrospectFixture(TestUmaServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Introspecting_RptToken_Then_Information_Are_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            // 1. Add resource.
            var addResource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");
            // 2. Add authorization policy.
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy
            {
                Rules = new List<PostPolicyRule>
                {
                    new PostPolicyRule
                    {
                        IsResourceOwnerConsentNeeded = false,
                        Scopes = new List<string>
                        {
                            "read"
                        },
                        ClientIdsAllowed = new List<string>
                        {
                            "client"
                        }
                    }
                },
                ResourceSetIds = new List<string>
                {
                    addResource.Id
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");
            // 3. Add the permission.
            var addPermission = await _permissionClient.AddByResolution(new PostPermission
            {
                ResourceSetId = addResource.Id,
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");
            // 4. Get RPT token
            var authResponse = await _authorizationClient.GetByResolution(new PostAuthorization
            {
                TicketId = addPermission.TicketId
            }, baseUrl + "/.well-known/uma-configuration", "header");

            // ACT
            // 5. Introspect the RPT token.
            var introspectionResult = await _introspectionClient.GetByResolution(authResponse.Rpt, baseUrl + "/.well-known/uma-configuration");

            // ASSERT
            Assert.NotNull(introspectionResult);
            Assert.True(introspectionResult.IsActive);
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
            _authorizationClient = new AuthorizationClient(new GetAuthorizationOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object));
            _permissionClient = new PermissionClient(
                new AddPermissionsOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object));
            _introspectionClient = new IntrospectionClient(
                new GetIntrospectionAction(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object));
        }
    }
}
