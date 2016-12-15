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
using SimpleIdentityServer.Client.Policy;
using SimpleIdentityServer.Client.ResourceSet;
using SimpleIdentityServer.Uma.Client.Factory;
using SimpleIdentityServer.Uma.Common.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace SimpleIdentityServer.Uma.Host.Tests
{
    public class PolicyFixture : IClassFixture<TestUmaServerFixture>
    {
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IPolicyClient _policyClient;
        private IResourceSetClient _resourceSetClient;
        private readonly TestUmaServerFixture _server;

        public PolicyFixture(TestUmaServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Adding_Policy_Then_Information_Can_Be_Retrieved()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var addResponse = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");

            // ACT
            var response = await _policyClient.AddByResolution(new PostPolicy
            {
                Rules = new List<PostPolicyRule>
                {
                    new PostPolicyRule
                    {
                        IsResourceOwnerConsentNeeded = false,
                        Claims = new List<PostClaim>
                        {
                            new PostClaim
                            {
                                Type = "role",
                                Value = "administrator"
                            }
                        },
                        Scopes = new List<string>
                        {
                            "read"
                        }
                    }
                },
                ResourceSetIds = new List<string>
                {
                    addResponse.Id
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");
            var information = await _policyClient.GetByResolution(response.PolicyId, baseUrl + "/.well-known/uma-configuration", "header");

            // ASSERT
            Assert.NotNull(response);
            Assert.False(string.IsNullOrWhiteSpace(response.PolicyId));
            Assert.NotNull(information);
            Assert.True(information.Rules.Count() == 1);
            Assert.True(information.ResourceSetIds.Count() == 1 && information.ResourceSetIds.First() == addResponse.Id);
            var rule = information.Rules.First();
            Assert.False(rule.IsResourceOwnerConsentNeeded);
            Assert.True(rule.Claims.Count() == 1);
            Assert.True(rule.Scopes.Count() == 1);
        }
        
        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _policyClient = new PolicyClient(new AddPolicyOperation(_httpClientFactoryStub.Object),
                new GetPolicyOperation(_httpClientFactoryStub.Object),
                new DeletePolicyOperation(_httpClientFactoryStub.Object),
                new GetPoliciesOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object));
            _resourceSetClient = new ResourceSetClient(new AddResourceSetOperation(_httpClientFactoryStub.Object),
                new DeleteResourceSetOperation(_httpClientFactoryStub.Object),
                new GetResourcesOperation(_httpClientFactoryStub.Object),
                new GetResourceOperation(_httpClientFactoryStub.Object),
                new UpdateResourceOperation(_httpClientFactoryStub.Object),
                new GetConfigurationOperation(_httpClientFactoryStub.Object));
        }
    }
}
