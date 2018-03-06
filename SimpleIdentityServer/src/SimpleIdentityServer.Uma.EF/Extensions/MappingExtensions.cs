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
using System.Collections.Generic;
using System.Linq;
using Domain = SimpleIdentityServer.Uma.Core.Models;
using Model = SimpleIdentityServer.Uma.EF.Models;

namespace SimpleIdentityServer.Uma.EF.Extensions
{
    internal static class MappingExtensions
    {
        public static Domain.ResourceSet ToDomain(this Model.ResourceSet resourceSet)
        {
            var policyIds = resourceSet.PolicyResources != null ?
                resourceSet.PolicyResources.Select(p => p.PolicyId)
                .ToList() : new List<string>();
            var policies = resourceSet.PolicyResources == null ? new Domain.Policy[0] :
                resourceSet.PolicyResources.Where(p => p.Policy != null).Select(p => p.Policy.ToDomain());
            return new Domain.ResourceSet
            {
                IconUri = resourceSet.IconUri,
                Name = resourceSet.Name,
                Scopes = GetList(resourceSet.Scopes),
                Id = resourceSet.Id,
                Type = resourceSet.Type,
                Uri = resourceSet.Uri,
                AuthorizationPolicyIds = policyIds,
                Policies = policies
            };
        }

        public static Domain.Policy ToDomain(this Model.Policy policy)
        {
            var rules = new List<Domain.PolicyRule>();
            var resourceSetIds = new List<string>();
            if (policy.Rules != null)
            {
                rules = policy.Rules.Select(r => r.ToDomain()).ToList();
            }

            if (policy.PolicyResources != null)
            {
                resourceSetIds = policy.PolicyResources
                    .Select(r => r.ResourceSetId)
                    .ToList();
            }

            return new Domain.Policy
            {
                Id = policy.Id,
                Rules = rules,
                ResourceSetIds = resourceSetIds
            };
        }

        public static Domain.PolicyRule ToDomain(this Model.PolicyRule policyRule)
        {
            var claims = new List<Domain.Claim>();
            if (policyRule.Claims != null &&
                policyRule.Claims.Any())
            {
                claims = JsonConvert.DeserializeObject<List<Domain.Claim>>(policyRule.Claims);
            }

            return new Domain.PolicyRule
            {
                Id = policyRule.Id,
                ClientIdsAllowed = GetList(policyRule.ClientIdsAllowed),
                Scopes = GetList(policyRule.Scopes),
                IsResourceOwnerConsentNeeded = policyRule.IsResourceOwnerConsentNeeded,
                Script = policyRule.Script,
                Claims = claims
            };
        }

        public static Model.ResourceSet ToModel(this Domain.ResourceSet resourceSet)
        {
            var policyIds = resourceSet.AuthorizationPolicyIds != null ?
                resourceSet.AuthorizationPolicyIds.Select(p =>
                    new Model.PolicyResource
                    {
                        ResourceSetId = resourceSet.Id,
                        PolicyId = p
                    }).ToList()
                : new List<Model.PolicyResource>();
            return new Model.ResourceSet
            {
                Id = resourceSet.Id,
                IconUri = resourceSet.IconUri,
                Name = resourceSet.Name,
                Scopes = GetConcatenatedList(resourceSet.Scopes),
                Type = resourceSet.Type,
                Uri = resourceSet.Uri,
                PolicyResources = policyIds
            };
        }

        public static Model.Policy ToModel(this Domain.Policy policy)
        {
            var rules = new List<Model.PolicyRule>();
            var resources = new List<Model.PolicyResource>();
            if (policy.Rules != null)
            {
                rules = policy.Rules.Select(r => r.ToModel()).ToList();
            }

            if (policy.ResourceSetIds != null)
            {
                resources = policy.ResourceSetIds.Select(r => new Model.PolicyResource
                {
                    PolicyId = policy.Id,
                    ResourceSetId = r
                }).ToList();
            }

            return new Model.Policy
            {
                Id = policy.Id,
                Rules = rules,
                PolicyResources = resources
            };
        }

        public static Model.PolicyRule ToModel(this Domain.PolicyRule policyRule)
        {
            var claims = string.Empty;
            if (policyRule.Claims != null && 
                policyRule.Claims.Any())
            {
                claims = JsonConvert.SerializeObject(policyRule.Claims);
            }

            return new Model.PolicyRule
            {
                Id = policyRule.Id,
                ClientIdsAllowed = GetConcatenatedList(policyRule.ClientIdsAllowed),
                Scopes = GetConcatenatedList(policyRule.Scopes),
                IsResourceOwnerConsentNeeded = policyRule.IsResourceOwnerConsentNeeded,
                Script = policyRule.Script,
                Claims = claims
            };
        }

        public static List<string> GetList(string concatenatedList)
        {

            var scopes = new List<string>();
            if (!string.IsNullOrEmpty(concatenatedList))
            {
                scopes = concatenatedList.Split(',').ToList();
            }

            return scopes;
        }

        public static string GetConcatenatedList(IEnumerable<string> list)
        {
            var concatenatedList = string.Empty;
            if (list != null && list.Any())
            {
                concatenatedList = string.Join(",", list);
            }

            return concatenatedList;
        }
    }
}
