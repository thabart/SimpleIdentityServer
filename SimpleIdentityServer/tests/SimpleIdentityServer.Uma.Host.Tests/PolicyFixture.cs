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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

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
        public async Task When_Adding_Policy_Then_Information_Can_Be_Returned()
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

        [Fact]
        public async Task When_Getting_All_Policies_Then_Identifiers_Are_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var addResource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy
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
                    addResource.Id
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");

            // ACT
            var response = await _policyClient.GetAllByResolution(baseUrl + "/.well-known/uma-configuration", "header");
            
            // ASSERT
            Assert.NotNull(response);
            Assert.True(response.Any(r => r == addPolicy.PolicyId));
        }
        
        [Fact]
        public async Task When_Removing_Policy_Then_Information_Doesnt_Exist()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var addResource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy
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
                    addResource.Id
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");

            // ACT
            var isRemoved = await _policyClient.DeleteByResolution(addPolicy.PolicyId, baseUrl + "/.well-known/uma-configuration", "header");
            var ex = await Assert.ThrowsAsync<HttpRequestException>(() => _policyClient.GetByResolution(addPolicy.PolicyId, baseUrl + "/.well-known/uma-configuration", "header"));

            // ASSERTS
            Assert.True(isRemoved);
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task When_Adding_Resource_To_Policy_Then_Changes_Are_Persisted()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var firstResource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");
            var secondResource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy
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
                    firstResource.Id
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");

            // ACT
            var isUpdated = await _policyClient.AddResourceByResolution(addPolicy.PolicyId, new PostAddResourceSet
            {
                ResourceSets = new List<string>
                {
                    secondResource.Id
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");
            var information = await _policyClient.GetByResolution(addPolicy.PolicyId, baseUrl + "/.well-known/uma-configuration", "header");

            // ASSERTS
            Assert.True(isUpdated);
            Assert.NotNull(information);
            Assert.True(information.ResourceSetIds.Count() == 2 && information.ResourceSetIds.All(r => r == firstResource.Id || r == secondResource.Id));
        }

        [Fact]
        public async Task When_Removing_Resource_From_Policy_Then_Changes_Are_Persisted()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var firstResource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");
            var secondResource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy
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
                    firstResource.Id,
                    secondResource.Id
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");

            // ACT
            var isUpdated = await _policyClient.DeleteResourceByResolution(addPolicy.PolicyId, secondResource.Id, baseUrl + "/.well-known/uma-configuration", "header");
            var information = await _policyClient.GetByResolution(addPolicy.PolicyId, baseUrl + "/.well-known/uma-configuration", "header");

            // ASSERTS
            Assert.True(isUpdated);
            Assert.NotNull(information);
            Assert.True(information.ResourceSetIds.Count() == 1 && information.ResourceSetIds.First() == firstResource.Id);
        }

        [Fact]
        public async Task When_Upading_Policy_Then_Changes_Are_Persistedd()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var firstResource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read",
                    "write"
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");
            var secondResource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy
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
                    firstResource.Id,
                    secondResource.Id
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");
            var firstInfo = await _policyClient.GetByResolution(addPolicy.PolicyId, baseUrl + "/.well-known/uma-configuration", "header");

            // ACT
            var isUpdated = await _policyClient.UpdateByResolution(new PutPolicy
            {
                PolicyId = firstInfo.Id,
                Rules = new List<PutPolicyRule>
                {
                    new PutPolicyRule
                    {
                       Id = firstInfo.Rules.First().Id,
                       IsResourceOwnerConsentNeeded = true,
                        Claims = new List<PostClaim>
                        {
                            new PostClaim
                            {
                                Type = "role",
                                Value = "administrator"
                            },
                            new PostClaim
                            {
                                Type = "role",
                                Value = "other"
                            }
                        },
                        Scopes = new List<string>
                        {
                            "read",
                            "write"
                        }
                    }
                }
            }, baseUrl + "/.well-known/uma-configuration", "header");
            var updatedInformation = await _policyClient.GetByResolution(addPolicy.PolicyId, baseUrl + "/.well-known/uma-configuration", "header");


            // ASSERTS
            Assert.True(isUpdated);
            Assert.NotNull(updatedInformation);
            Assert.True(updatedInformation.Rules.Count() == 1);
            var rule = updatedInformation.Rules.First();
            Assert.True(rule.IsResourceOwnerConsentNeeded);
            Assert.True(rule.Claims.Count() == 2);
            Assert.True(rule.Scopes.Count() == 2);
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
        }
    }
}
