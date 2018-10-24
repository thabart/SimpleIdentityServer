using SimpleIdentityServer.Uma.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Uma.Core.Extensions
{
    public static class CopyExtensions
    {
        public static ResourceSet Copy(this ResourceSet resourceSet)
        {
            return new ResourceSet
            {
                AuthorizationPolicyIds = resourceSet.AuthorizationPolicyIds == null ? new List<string>() : resourceSet.AuthorizationPolicyIds.ToList(),
                IconUri = resourceSet.IconUri,
                Id = resourceSet.Id,
                Name = resourceSet.Name,
                Type = resourceSet.Type,
                Uri = resourceSet.Uri,
                Policies = resourceSet.Policies == null ? new List<Policy>() : resourceSet.Policies.Select(p => p.Copy()).ToList(),
                Scopes = resourceSet.Scopes == null ? new List<string>() : resourceSet.Scopes.ToList()
            };
        }

        public static Policy Copy(this Policy policy)
        {
            return new Policy
            {
                Id = policy.Id,
                Rules = policy.Rules == null ? new List<PolicyRule>() : policy.Rules.Select(r => r.Copy()).ToList(),
                ResourceSetIds = policy.ResourceSetIds == null ? new List<string>() : policy.ResourceSetIds.ToList()
            };
        }

        public static PolicyRule Copy(this PolicyRule policyRule)
        {
            return new PolicyRule
            {
                ClientIdsAllowed = policyRule.ClientIdsAllowed == null ? new List<string>() : policyRule.ClientIdsAllowed.ToList(),
                Id = policyRule.Id,
                IsResourceOwnerConsentNeeded = policyRule.IsResourceOwnerConsentNeeded, 
                Scopes = policyRule.Scopes == null ? new List<string>() : policyRule.Scopes.ToList(),
                Script = policyRule.Script,
                OpenIdProvider = policyRule.OpenIdProvider,
                Claims = policyRule.Claims == null ? new List<Claim>() : policyRule.Claims.Select(c =>
                    new Claim
                    {
                        Type = c.Type,
                        Value = c.Value
                    }
                ).ToList()
            };
        }
    }
}