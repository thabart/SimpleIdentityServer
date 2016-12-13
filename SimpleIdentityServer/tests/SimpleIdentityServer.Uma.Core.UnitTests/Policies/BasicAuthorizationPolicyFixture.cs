#region copyright
// Copyright 2015 Habart Thierry
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
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Uma.Core.JwtToken;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Policies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Policies
{
    public class BasicAuthorizationPolicyFixture
    {
        private Mock<IParametersProvider> _parametersProviderStub;
        private Mock<IIdentityServerClientFactory> _identityServerClientFactoryStub;
        private Mock<IJwtTokenParser> _jwtTokenParserStub;
        private IBasicAuthorizationPolicy _basicAuthorizationPolicy;

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.ThrowsAsync<ArgumentNullException>(() => _basicAuthorizationPolicy.Execute(null, null, null));
            Assert.ThrowsAsync<ArgumentNullException>(() => _basicAuthorizationPolicy.Execute(new Ticket(), null, null));
        }
        
        [Fact]
        public async Task When_Doesnt_have_Permission_To_Access_To_Scope_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new Ticket
            {
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicy = new Policy
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        Scopes = new List<string>
                        {
                            "read"
                        }
                    }
                }
            };

            // ACT
            var result = await _basicAuthorizationPolicy.Execute(ticket, authorizationPolicy, null);

            // ASSERT
            Assert.True(result.Type == AuthorizationPolicyResultEnum.NotAuthorized);
        }

        [Fact]
        public async Task When_Client_Is_Not_Allowed_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new Ticket
            {
                ClientId = "invalid_client_id",
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicy = new Policy
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        ClientIdsAllowed = new List<string>
                        {
                            "client_id"
                        },
                        Scopes = new List<string>
                        {
                            "read",
                            "create",
                            "update"
                        }
                    }
                }

            };
            
            // ACT
            var result = await _basicAuthorizationPolicy.Execute(ticket, authorizationPolicy, null);

            // ASSERT
            Assert.True(result.Type == AuthorizationPolicyResultEnum.NotAuthorized);
        }

        [Fact]
        public async Task When_There_Is_No_Access_Token_Passed_Then_NeedInfo_Is_Returned()
        {
            // ARRANGE
            const string configurationUrl = "http://localhost/configuration";
            InitializeFakeObjects();
            var ticket = new Ticket
            {
                ClientId = "client_id",
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicy = new Policy
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        ClientIdsAllowed = new List<string>
                        {
                            "client_id"
                        },
                        Scopes = new List<string>
                        {
                            "read",
                            "create",
                            "update"
                        },
                        Claims = new List<Claim>
                        {
                            new Claim
                            {
                                Type = "name"
                            },
                            new Claim
                            {
                                Type = "email"
                            }
                        }
                    }
                }
            };
            var claimTokenParameters = new List<ClaimTokenParameter>
            {
                new ClaimTokenParameter
                {
                    Format = "bad_format",
                    Token = "token"
                }
            };
            _parametersProviderStub.Setup(p => p.GetOpenIdConfigurationUrl())
                .Returns(configurationUrl);

            // ACT
            var result = await _basicAuthorizationPolicy.Execute(ticket, authorizationPolicy, claimTokenParameters);

            // ASSERT
            Assert.True(result.Type == AuthorizationPolicyResultEnum.NeedInfo);
            var errorDetails = result.ErrorDetails as Dictionary<string, object>;
            Assert.NotNull(errorDetails);
            Assert.True(errorDetails.ContainsKey(Constants.ErrorDetailNames.RequestingPartyClaims));
            var requestingPartyClaims = errorDetails[Constants.ErrorDetailNames.RequestingPartyClaims] as Dictionary<string, object>;
            Assert.NotNull(requestingPartyClaims);
            Assert.True(requestingPartyClaims.ContainsKey(Constants.ErrorDetailNames.RequiredClaims));
            Assert.True(requestingPartyClaims.ContainsKey(Constants.ErrorDetailNames.RedirectUser));
            var requiredClaims = requestingPartyClaims[Constants.ErrorDetailNames.RequiredClaims] as List<Dictionary<string, string>>;
            Assert.NotNull(requiredClaims);
            Assert.True(requiredClaims.Any(r => r.Any(kv => kv.Key == Constants.ErrorDetailNames.ClaimName && kv.Value == "name")));
            Assert.True(requiredClaims.Any(r => r.Any(kv => kv.Key == Constants.ErrorDetailNames.ClaimFriendlyName && kv.Value == "name")));
            Assert.True(requiredClaims.Any(r => r.Any(kv => kv.Key == Constants.ErrorDetailNames.ClaimName && kv.Value == "email")));
            Assert.True(requiredClaims.Any(r => r.Any(kv => kv.Key == Constants.ErrorDetailNames.ClaimFriendlyName && kv.Value == "email")));
        }

        [Fact]
        public async Task When_JwsPayload_Cannot_Be_Extracted_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            const string configurationUrl = "http://localhost/configuration";
            InitializeFakeObjects();
            var ticket = new Ticket
            {
                ClientId = "client_id",
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicy = new Policy
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        ClientIdsAllowed = new List<string>
                        {
                            "client_id"
                        },
                        Scopes = new List<string>
                        {
                            "read",
                            "create",
                            "update"
                        },
                        Claims = new List<Claim>
                        {
                            new Claim
                            {
                                Type = "name"
                            },
                            new Claim
                            {
                                Type = "email"
                            }
                        }
                    }
                }
            };
            var claimTokenParameters = new List<ClaimTokenParameter>
            {
                new ClaimTokenParameter
                {
                    Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                    Token = "token"
                }
            };
            _parametersProviderStub.Setup(p => p.GetOpenIdConfigurationUrl())
                .Returns(configurationUrl);
            _jwtTokenParserStub.Setup(j => j.UnSign(It.IsAny<string>()))
                .Returns(Task.FromResult<JwsPayload>(null));

            // ACT
            var result = await _basicAuthorizationPolicy.Execute(ticket, authorizationPolicy, claimTokenParameters);

            // ASSERT
            Assert.True(result.Type == AuthorizationPolicyResultEnum.NotAuthorized);
        }

        [Fact]
        public async Task When_Role_Is_Not_Correct_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            const string configurationUrl = "http://localhost/configuration";
            InitializeFakeObjects();
            var ticket = new Ticket
            {
                ClientId = "client_id",
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicy = new Policy
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        ClientIdsAllowed = new List<string>
                        {
                            "client_id"
                        },
                        Scopes = new List<string>
                        {
                            "read",
                            "create",
                            "update"
                        },
                        Claims = new List<Claim>
                        {
                            new Claim
                            {
                                Type = "role",
                                Value = "role1"
                            },
                            new Claim
                            {
                                Type = "role",
                                Value = "role2"
                            }
                        }
                    }
                }
            };
            var claimTokenParameters = new List<ClaimTokenParameter>
            {
                new ClaimTokenParameter
                {
                    Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                    Token = "token"
                }
            };
            _parametersProviderStub.Setup(p => p.GetOpenIdConfigurationUrl())
                .Returns(configurationUrl);
            _jwtTokenParserStub.Setup(j => j.UnSign(It.IsAny<string>()))
                .Returns(Task.FromResult(new JwsPayload
                {
                    {
                        "role", "role1,role3"
                    }
                }));

            // ACT
            var result = await _basicAuthorizationPolicy.Execute(ticket, authorizationPolicy, claimTokenParameters);

            // ASSERT
            Assert.True(result.Type == AuthorizationPolicyResultEnum.NotAuthorized);
        }

        [Fact]
        public async Task When_There_Is_No_Role_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            const string configurationUrl = "http://localhost/configuration";
            InitializeFakeObjects();
            var ticket = new Ticket
            {
                ClientId = "client_id",
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicy = new Policy
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        ClientIdsAllowed = new List<string>
                        {
                            "client_id"
                        },
                        Scopes = new List<string>
                        {
                            "read",
                            "create",
                            "update"
                        },
                        Claims = new List<Claim>
                        {
                            new Claim
                            {
                                Type = "role",
                                Value = "role1"
                            },
                            new Claim
                            {
                                Type = "role",
                                Value = "role2"
                            }
                        }
                    }
                }
            };
            var claimTokenParameters = new List<ClaimTokenParameter>
            {
                new ClaimTokenParameter
                {
                    Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                    Token = "token"
                }
            };
            _parametersProviderStub.Setup(p => p.GetOpenIdConfigurationUrl())
                .Returns(configurationUrl);
            _jwtTokenParserStub.Setup(j => j.UnSign(It.IsAny<string>()))
                .Returns(Task.FromResult(new JwsPayload()));

            // ACT
            var result = await _basicAuthorizationPolicy.Execute(ticket, authorizationPolicy, claimTokenParameters);

            // ASSERT
            Assert.True(result.Type == AuthorizationPolicyResultEnum.NotAuthorized);
        }

        [Fact]
        public async Task When_Passing_Not_Valid_Roles_In_JArray_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            const string configurationUrl = "http://localhost/configuration";
            InitializeFakeObjects();
            var ticket = new Ticket
            {
                ClientId = "client_id",
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicy = new Policy
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        ClientIdsAllowed = new List<string>
                        {
                            "client_id"
                        },
                        Scopes = new List<string>
                        {
                            "read",
                            "create",
                            "update"
                        },
                        Claims = new List<Claim>
                        {
                            new Claim
                            {
                                Type = "role",
                                Value = "role1"
                            },
                            new Claim
                            {
                                Type = "role",
                                Value = "role2"
                            }
                        }
                    }
                }
            };
            var claimTokenParameters = new List<ClaimTokenParameter>
            {
                new ClaimTokenParameter
                {
                    Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                    Token = "token"
                }
            };
            _parametersProviderStub.Setup(p => p.GetOpenIdConfigurationUrl())
                .Returns(configurationUrl);
            var payload = new JwsPayload();
            payload.Add("role", new JArray("role3"));
            _jwtTokenParserStub.Setup(j => j.UnSign(It.IsAny<string>()))
                .Returns(Task.FromResult(payload));

            // ACT
            var result = await _basicAuthorizationPolicy.Execute(ticket, authorizationPolicy, claimTokenParameters);

            // ASSERT
            Assert.True(result.Type == AuthorizationPolicyResultEnum.NotAuthorized);
        }
    
        [Fact]
        public async Task When_Passing_Not_Valid_Roles_InStringArray_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            const string configurationUrl = "http://localhost/configuration";
            InitializeFakeObjects();
            var ticket = new Ticket
            {
                ClientId = "client_id",
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicy = new Policy
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        ClientIdsAllowed = new List<string>
                        {
                            "client_id"
                        },
                        Scopes = new List<string>
                        {
                            "read",
                            "create",
                            "update"
                        },
                        Claims = new List<Claim>
                        {
                            new Claim
                            {
                                Type = "role",
                                Value = "role1"
                            },
                            new Claim
                            {
                                Type = "role",
                                Value = "role2"
                            }
                        }
                    }
                }
            };
            var claimTokenParameters = new List<ClaimTokenParameter>
            {
                new ClaimTokenParameter
                {
                    Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                    Token = "token"
                }
            };
            _parametersProviderStub.Setup(p => p.GetOpenIdConfigurationUrl())
                .Returns(configurationUrl);
            var payload = new JwsPayload();
            payload.Add("role", new string[] { "role3" });
            _jwtTokenParserStub.Setup(j => j.UnSign(It.IsAny<string>()))
                .Returns(Task.FromResult(payload));

            // ACT
            var result = await _basicAuthorizationPolicy.Execute(ticket, authorizationPolicy, claimTokenParameters);

            // ASSERT
            Assert.True(result.Type == AuthorizationPolicyResultEnum.NotAuthorized);
        }

        [Fact]
        public async Task When_Claims_Are_Not_Corred_Then_NotAuthorized_Is_Returned()
        {
            // ARRANGE
            const string configurationUrl = "http://localhost/configuration";
            InitializeFakeObjects();
            var ticket = new Ticket
            {
                ClientId = "client_id",
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicy = new Policy
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        ClientIdsAllowed = new List<string>
                        {
                            "client_id"
                        },
                        Scopes = new List<string>
                        {
                            "read",
                            "create",
                            "update"
                        },
                        Claims = new List<Claim>
                        {
                            new Claim
                            {
                                Type = "name",
                                Value = "name"
                            },
                            new Claim
                            {
                                Type = "email",
                                Value = "email"
                            }
                        }
                    }
                }
            };
            var claimTokenParameters = new List<ClaimTokenParameter>
            {
                new ClaimTokenParameter
                {
                    Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                    Token = "token"
                }
            };
            _parametersProviderStub.Setup(p => p.GetOpenIdConfigurationUrl())
                .Returns(configurationUrl);
            _jwtTokenParserStub.Setup(j => j.UnSign(It.IsAny<string>()))
                .Returns(Task.FromResult(new JwsPayload
                {
                    {
                        "name", "bad_name"
                    }
                }));

            // ACT
            var result = await _basicAuthorizationPolicy.Execute(ticket, authorizationPolicy, claimTokenParameters);

            // ASSERT
            Assert.True(result.Type == AuthorizationPolicyResultEnum.NotAuthorized);
        }
        
        [Fact]
        public async Task When_ResourceOwnerConsent_Is_Required_Then_RequestSubmitted_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new Ticket
            {
                ClientId = "client_id",
                IsAuthorizedByRo = false,
                Scopes = new List<string>
                {
                    "read",
                    "create",
                    "update"
                }
            };

            var authorizationPolicy = new Policy
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        ClientIdsAllowed = new List<string>
                        {
                            "client_id"
                        },
                        IsResourceOwnerConsentNeeded = true,
                        Scopes = new List<string>
                        {
                            "read",
                            "create",
                            "update"
                        }
                    }
                }
            };

            // ACT
            var result = await _basicAuthorizationPolicy.Execute(ticket, authorizationPolicy, null);

            // ASSERT
            Assert.True(result.Type == AuthorizationPolicyResultEnum.RequestSubmitted);
        }

        [Fact]
        public async Task When_AuthorizationPassed_Then_Authorization_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var ticket = new Ticket
            {
                ClientId = "client_id",
                IsAuthorizedByRo = true,
                Scopes = new List<string>
                {
                    "create"
                }
            };

            var authorizationPolicy = new Policy
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        ClientIdsAllowed = new List<string>
                        {
                            "client_id"
                        },
                        IsResourceOwnerConsentNeeded = true,
                        Scopes = new List<string>
                        {
                            "create"
                        }
                    }
                }
            };

            // ACT
            var result = await _basicAuthorizationPolicy.Execute(ticket, authorizationPolicy, null);

            // ASSERT
            Assert.True(result.Type == AuthorizationPolicyResultEnum.Authorized);
        }

        private void InitializeFakeObjects()
        {
            _parametersProviderStub = new Mock<IParametersProvider>();
            _identityServerClientFactoryStub = new Mock<IIdentityServerClientFactory>();
            _jwtTokenParserStub = new Mock<IJwtTokenParser>();
            _basicAuthorizationPolicy = new BasicAuthorizationPolicy(_parametersProviderStub.Object,
                _identityServerClientFactoryStub.Object,
                _jwtTokenParserStub.Object);
        }
    }
}
