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

using Newtonsoft.Json;
using SimpleIdentityServer.Uma.EF;
using SimpleIdentityServer.Uma.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Uma.Startup.Extensions
{
    public static class SimpleIdServerUmaContextExtensions
    {
        private static string _firstPolicyId = "986ea7da-d911-48b8-adfa-124b3827246a";

        #region Public static methods

        public static void EnsureSeedData(this SimpleIdServerUmaContext context)
        {
            InsertResources(context);
            InsertPolicies(context);
            context.SaveChanges();
        }

        #endregion

        #region Private static methods

        private static void InsertResources(SimpleIdServerUmaContext context)
        {
            if (!context.ResourceSets.Any())
            {
                context.ResourceSets.AddRange(new[]
                {
                    new ResourceSet
                    {
                        Id = "1",
                        Name = "Personnal information",
                        Scopes = "read,write,execute"
                    }
                });
            }
        }

        private static void InsertPolicies(SimpleIdServerUmaContext context)
        {
            if (!context.Policies.Any())
            {
                var claims = new List<Core.Models.Claim>();
                claims.Add(new Core.Models.Claim { Type = "sub", Value = "administrator" });
                context.Policies.AddRange(new[]
                {
                    new Policy
                    {
                        Id = _firstPolicyId,
                        PolicyResources = new List<PolicyResource>
                        {
                            new PolicyResource
                            {
                                PolicyId = _firstPolicyId,
                                ResourceSetId = "1"
                            }
                        },
                        Rules = new List<PolicyRule>
                        {
                            new PolicyRule
                            {
                                Id = Guid.NewGuid().ToString(),
                                PolicyId = _firstPolicyId,
                                IsResourceOwnerConsentNeeded = false,
                                ClientIdsAllowed = "client",
                                Scopes = "read",
                                Claims = JsonConvert.SerializeObject(claims)
                            }
                        }
                    }
                });
            }
        }

        #endregion
    }
}
