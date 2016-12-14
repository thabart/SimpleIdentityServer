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

using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Host.DTOs.Requests;
using SimpleIdentityServer.Uma.Host.DTOs.Responses;

using DomainResponse = SimpleIdentityServer.Uma.Core.Responses;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Uma.Host.Extensions
{
    internal static class MappingExtensions
    {
        public static AddResouceSetParameter ToParameter(this PostResourceSet postResourceSet)
        {
            return new AddResouceSetParameter
            {
                IconUri = postResourceSet.IconUri,
                Name = postResourceSet.Name,
                Scopes = postResourceSet.Scopes,
                Type = postResourceSet.Type,
                Uri = postResourceSet.Uri
            };
        }

        public static UpdateResourceSetParameter ToParameter(this PutResourceSet putResourceSet)
        {
            return new UpdateResourceSetParameter
            {
                Id = putResourceSet.Id,
                Name = putResourceSet.Name,
                IconUri = putResourceSet.IconUri,
                Scopes = putResourceSet.Scopes,
                Type = putResourceSet.Type,
                Uri = putResourceSet.Uri
            };
        }

        public static AddScopeParameter ToParameter(this PostScope postScope)
        {
            return new AddScopeParameter
            {
                Id = postScope.Id,
                Name = postScope.Name,
                IconUri = postScope.IconUri
            };
        }

        public static UpdateScopeParameter ToParameter(this PutScope putScope)
        {
            return new UpdateScopeParameter
            {
                Id = putScope.Id,
                Name = putScope.Name,
                IconUri = putScope.IconUri
            };
        }

        public static AddPermissionParameter ToParameter(this PostPermission postPermission)
        {
            return new AddPermissionParameter
            {
                ResourceSetId = postPermission.ResourceSetId,
                Scopes = postPermission.Scopes
            };
        }

        public static GetAuthorizationActionParameter ToParameter(this PostAuthorization postAuthorization)
        {
            var tokens = new List<ClaimTokenParameter>();
            if (postAuthorization.ClaimTokens != null &&
                postAuthorization.ClaimTokens.Any())
            {
                tokens = postAuthorization.ClaimTokens.Select(ct => ct.ToParameter()).ToList();
            }

            return new GetAuthorizationActionParameter
            {
                Rpt = postAuthorization.Rpt,
                TicketId = postAuthorization.TicketId,
                ClaimTokenParameters = tokens
            };
        }

        public static ClaimTokenParameter ToParameter(this PostClaimToken postClaimToken)
        {
            return new ClaimTokenParameter
            {
                Format = postClaimToken.Format,
                Token = postClaimToken.Token
            };
        }

        public static AddPolicyParameter ToParameter(this PostPolicy postPolicy)
        {
            var rules = postPolicy.Rules == null ? new List<AddPolicyRuleParameter>()
                : postPolicy.Rules.Select(r => r.ToParameter()).ToList();
            return new AddPolicyParameter
            {
                Rules = rules,
                ResourceSetIds = postPolicy.ResourceSetIds
            };
        }

        public static AddPolicyRuleParameter ToParameter(this PostPolicyRule policyRule)
        {
            var claims = policyRule.Claims == null ? new List<AddClaimParameter>()
                : policyRule.Claims.Select(p => p.ToParameter()).ToList();
            return new AddPolicyRuleParameter
            {
                Claims = claims,
                ClientIdsAllowed = policyRule.ClientIdsAllowed,
                IsResourceOwnerConsentNeeded = policyRule.IsResourceOwnerConsentNeeded,
                Scopes = policyRule.Scopes,
                Script = policyRule.Script
            };
        }

        public static AddClaimParameter ToParameter(this PostClaim postClaim)
        {
            return new AddClaimParameter
            {
                Type = postClaim.Type,
                Value = postClaim.Value
            };
        }

        public static UpdatePolicyParameter ToParameter(this PutPolicy putPolicy)
        {
            var rules = putPolicy.Rules == null ? new List<UpdatePolicyRuleParameter>()
                : putPolicy.Rules.Select(r => r.ToParameter()).ToList();
            return new UpdatePolicyParameter
            {
                PolicyId = putPolicy.PolicyId,
                Rules = rules
            };
        }

        public static UpdatePolicyRuleParameter ToParameter(this PutPolicyRule policyRule)
        {
            var claims = policyRule.Claims == null ? new List<AddClaimParameter>()
                : policyRule.Claims.Select(p => p.ToParameter()).ToList();
            return new UpdatePolicyRuleParameter
            {
                Claims = claims,
                ClientIdsAllowed = policyRule.ClientIdsAllowed,
                Id = policyRule.Id,
                IsResourceOwnerConsentNeeded = policyRule.IsResourceOwnerConsentNeeded,
                Scopes = policyRule.Scopes,
                Script = policyRule.Script
            };
        }

        public static ResourceSetResponse ToResponse(this ResourceSet resourceSet)
        {
            return new ResourceSetResponse
            {
                Id = resourceSet.Id,
                IconUri = resourceSet.IconUri,
                Name = resourceSet.Name,
                Scopes = resourceSet.Scopes,
                Type = resourceSet.Type,
                Uri = resourceSet.Uri 
            };
        }

        public static ScopeResponse ToResponse(this Scope scope)
        {
            return new ScopeResponse
            {
                IconUri = scope.IconUri,
                Name = scope.Name
            };
        }

        public static PolicyResponse ToResponse(this Policy policy)
        {
            var rules = policy.Rules == null ? new List<PolicyRuleResponse>()
                : policy.Rules.Select(p => p.ToResponse()).ToList();
            return new PolicyResponse
            {
                Id = policy.Id,
                ResourceSetIds = policy.ResourceSetIds,
                Rules = rules
            };
        }

        public static PolicyRuleResponse ToResponse(this PolicyRule policyRule)
        {
            var claims = policyRule.Claims == null ? new List<PostClaim>()
                : policyRule.Claims.Select(p => p.ToResponse()).ToList();
            return new PolicyRuleResponse
            {
                Id = policyRule.Id,
                Claims = claims,
                ClientIdsAllowed = policyRule.ClientIdsAllowed,
                IsResourceOwnerConsentNeeded = policyRule.IsResourceOwnerConsentNeeded,
                Scopes = policyRule.Scopes,
                Script = policyRule.Script
            };
        }

        public static PostClaim ToResponse(this Claim claim)
        {
            return new PostClaim
            {
                Type = claim.Type,
                Value = claim.Value
            };
        }

        public static ConfigurationResponse ToResponse(this DomainResponse.ConfigurationResponse configuration)
        {
            return new ConfigurationResponse
            {
                AatGrantTypesSupported = configuration.AatGrantTypesSupported,
                AatProfilesSupported = configuration.AatProfilesSupported,
                AuthorizationEndPoint = configuration.AuthorizationEndPoint,
                ClaimTokenProfilesSupported = configuration.ClaimTokenProfilesSupported,
                DynamicClientEndPoint = configuration.DynamicClientEndPoint,
                IntrospectionEndPoint = configuration.IntrospectionEndPoint,
                Issuer = configuration.Issuer,
                PatGrantTypesSupported = configuration.PatGrantTypesSupported,
                PatProfilesSupported = configuration.PatProfilesSupported,
                PermissionRegistrationEndPoint = configuration.PermissionRegistrationEndPoint,
                PolicyEndPoint = configuration.PolicyEndPoint,
                RequestingPartyClaimsEndPoint = configuration.RequestingPartyClaimsEndPoint,
                ResourceSetRegistrationEndPoint = configuration.ResourceSetRegistrationEndPoint,
                RtpEndPoint = configuration.RtpEndPoint,
                RtpProfilesSupported = configuration.RtpProfilesSupported,
                TokenEndPoint = configuration.TokenEndPoint,
                UmaProfilesSupported = configuration.UmaProfilesSupported,
                Version = configuration.Version
            };
        }
    }
}
