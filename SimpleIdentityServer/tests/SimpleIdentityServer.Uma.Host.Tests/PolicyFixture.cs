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
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Uma.Client.Policy;
using SimpleIdentityServer.Uma.Client.ResourceSet;
using SimpleIdentityServer.Uma.Common.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Host.Tests
{
    public class PolicyFixture : IClassFixture<TestUmaServerFixture>
    {
        const string baseUrl = "http://localhost:5000";
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IPolicyClient _policyClient;
        private IResourceSetClient _resourceSetClient;
        private readonly TestUmaServerFixture _server;

        public PolicyFixture(TestUmaServerFixture server)
        {
            _server = server;
        }

        #region Errors

        #region Add

        [Fact]
        public async Task When_Add_Policy_And_Pass_No_ResourceIds_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var response = await _policyClient.AddByResolution(new PostPolicy
            {
                ResourceSetIds = null
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("invalid_request", response.Error.Error);
            Assert.Equal("the parameter resource_set_ids needs to be specified", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Policy_And_Pass_No_Rules_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var response = await _policyClient.AddByResolution(new PostPolicy
            {
                ResourceSetIds = new List<string>
                {
                    "resource_id"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("invalid_request", response.Error.Error);
            Assert.Equal("the parameter rules needs to be specified", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Policy_And_ResourceOwner_Doesnt_Exists_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var response = await _policyClient.AddByResolution(new PostPolicy
            {
                ResourceSetIds = new List<string>
                {
                    "resource_id"
                },
                Rules = new List<PostPolicyRule>
                {
                    new PostPolicyRule
                    {

                    }
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("invalid_resource_set_id", response.Error.Error);
            Assert.Equal("resource set resource_id doesn't exist", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Policy_And_Scope_Doesnt_Exists_Then_Error_Is_Returned()
        {
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
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ACT
            var response = await _policyClient.AddByResolution(new PostPolicy
            {
                ResourceSetIds = new List<string>
                {
                    addResponse.Content.Id
                },
                Rules = new List<PostPolicyRule>
                {
                    new PostPolicyRule
                    {
                        Scopes = new List<string>
                        {
                            "scope"
                        }
                    }
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("invalid_scope", response.Error.Error);
            Assert.Equal("one or more scopes don't belong to a resource set", response.Error.ErrorDescription);
        }

        #endregion

        #endregion


        #region Happy path

        #region Add

        [Fact]
        public async Task When_Adding_Policy_Then_Information_Can_Be_Returned()
        {
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
            }, baseUrl + "/.well-known/uma2-configuration", "header");

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
                    addResponse.Content.Id
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");
            var information = await _policyClient.GetByResolution(response.Content.PolicyId, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERT
            Assert.NotNull(response);
            Assert.False(string.IsNullOrWhiteSpace(response.Content.PolicyId));
            Assert.NotNull(information);
            Assert.True(information.Content.Rules.Count() == 1);
            Assert.True(information.Content.ResourceSetIds.Count() == 1 && information.Content.ResourceSetIds.First() == addResponse.Content.Id);
            var rule = information.Content.Rules.First();
            Assert.False(rule.IsResourceOwnerConsentNeeded);
            Assert.True(rule.Claims.Count() == 1);
            Assert.True(rule.Scopes.Count() == 1);
        }

        #endregion

        #region Get all

        [Fact]
        public async Task When_Getting_All_Policies_Then_Identifiers_Are_Returned()
        {
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
            }, baseUrl + "/.well-known/uma2-configuration", "header");
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
                    addResource.Content.Id
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ACT
            var response = await _policyClient.GetAllByResolution(baseUrl + "/.well-known/uma2-configuration", "header");
            
            // ASSERT
            Assert.NotNull(response);
            Assert.True(response.Content.Any(r => r == addPolicy.Content.PolicyId));
        }

        #endregion

        #region Remove

        [Fact]
        public async Task When_Removing_Policy_Then_Information_Doesnt_Exist()
        {
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
            }, baseUrl + "/.well-known/uma2-configuration", "header");
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
                    addResource.Content.Id
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ACT
            var isRemoved = await _policyClient.DeleteByResolution(addPolicy.Content.PolicyId, baseUrl + "/.well-known/uma2-configuration", "header");
            var ex = await _policyClient.GetByResolution(addPolicy.Content.PolicyId, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERTS
            Assert.False(isRemoved.ContainsError);
            Assert.True(ex.ContainsError);
            Assert.NotNull(ex);
        }

        #endregion

        #region Add

        [Fact]
        public async Task When_Adding_Resource_To_Policy_Then_Changes_Are_Persisted()
        {
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
            }, baseUrl + "/.well-known/uma2-configuration", "header");
            var secondResource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");
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
                    firstResource.Content.Id
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ACT
            var isUpdated = await _policyClient.AddResourceByResolution(addPolicy.Content.PolicyId, new PostAddResourceSet
            {
                ResourceSets = new List<string>
                {
                    secondResource.Content.Id
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");
            var information = await _policyClient.GetByResolution(addPolicy.Content.PolicyId, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERTS
            Assert.False(isUpdated.ContainsError);
            Assert.NotNull(information);
            Assert.True(information.Content.ResourceSetIds.Count() == 2 && information.Content.ResourceSetIds.All(r => r == firstResource.Content.Id || r == secondResource.Content.Id));
        }

        #endregion

        #region Remove from policy

        [Fact]
        public async Task When_Removing_Resource_From_Policy_Then_Changes_Are_Persisted()
        {
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
            }, baseUrl + "/.well-known/uma2-configuration", "header");
            var secondResource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");
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
                    firstResource.Content.Id,
                    secondResource.Content.Id
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");

            // ACT
            var isUpdated = await _policyClient.DeleteResourceByResolution(addPolicy.Content.PolicyId, secondResource.Content.Id, baseUrl + "/.well-known/uma2-configuration", "header");
            var information = await _policyClient.GetByResolution(addPolicy.Content.PolicyId, baseUrl + "/.well-known/uma2-configuration", "header");

            // ASSERTS
            Assert.False(isUpdated.ContainsError);
            Assert.NotNull(information);
            Assert.True(information.Content.ResourceSetIds.Count() == 1 && information.Content.ResourceSetIds.First() == firstResource.Content.Id);
        }

        #endregion

        #region Update

        [Fact]
        public async Task When_Updating_Policy_Then_Changes_Are_Persistedd()
        {
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
            }, baseUrl + "/.well-known/uma2-configuration", "header");
            var secondResource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                {
                    "read"
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");
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
                    firstResource.Content.Id,
                    secondResource.Content.Id
                }
            }, baseUrl + "/.well-known/uma2-configuration", "header");
            var firstInfo = await _policyClient.GetByResolution(addPolicy.Content.PolicyId, baseUrl + "/.well-known/uma2-configuration", "header");

            // ACT
            var isUpdated = await _policyClient.UpdateByResolution(new PutPolicy
            {
                PolicyId = firstInfo.Content.Id,
                Rules = new List<PutPolicyRule>
                {
                    new PutPolicyRule
                    {
                       Id = firstInfo.Content.Rules.First().Id,
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
            }, baseUrl + "/.well-known/uma2-configuration", "header");
            var updatedInformation = await _policyClient.GetByResolution(addPolicy.Content.PolicyId, baseUrl + "/.well-known/uma2-configuration", "header");


            // ASSERTS
            Assert.False(isUpdated.ContainsError);
            Assert.NotNull(updatedInformation);
            Assert.True(updatedInformation.Content.Rules.Count() == 1);
            var rule = updatedInformation.Content.Rules.First();
            Assert.True(rule.IsResourceOwnerConsentNeeded);
            Assert.True(rule.Claims.Count() == 2);
            Assert.True(rule.Scopes.Count() == 2);
        }

        #endregion

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
        }
    }
}
