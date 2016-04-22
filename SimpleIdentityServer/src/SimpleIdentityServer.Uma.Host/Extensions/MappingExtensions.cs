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

namespace SimpleIdentityServer.Uma.Host.Extensions
{
    internal static class MappingExtensions
    {
        #region To parameters

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
            return new GetAuthorizationActionParameter
            {
                Rpt = postAuthorization.Rpt,
                TicketId = postAuthorization.TicketId
            };
        }

        public static AddPolicyParameter ToParameter(this PostPolicy postPolicy)
        {
            return new AddPolicyParameter
            {
                ClientIdsAllowed = postPolicy.ClientIdsAllowed,
                IsCustom = postPolicy.IsCustom,
                IsResourceOwnerConsentNeeded = postPolicy.IsResourceOwnerConsentNeeded,
                ResourceSetIds = postPolicy.ResourceSetIds,
                Scopes = postPolicy.Scopes,
                Script = postPolicy.Script
            };
        }

        #endregion

        #region To responses

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
            return new PolicyResponse
            {
                Id = policy.Id,
                ClientIdsAllowed = policy.ClientIdsAllowed,
                IsCustom = policy.IsCustom,
                IsResourceOwnerConsentNeeded = policy.IsResourceOwnerConsentNeeded,
                ResourceSetIds = policy.ResourceSetIds
            };
        }

        #endregion
    }
}
