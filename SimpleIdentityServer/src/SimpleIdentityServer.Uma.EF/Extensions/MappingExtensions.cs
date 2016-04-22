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

using System.Collections.Generic;
using System.Linq;
using Domain = SimpleIdentityServer.Uma.Core.Models;
using Model = SimpleIdentityServer.Uma.EF.Models;

namespace SimpleIdentityServer.Uma.EF.Extensions
{
    internal static class MappingExtensions
    {
        #region To Domains

        public static Domain.ResourceSet ToDomain(this Model.ResourceSet resourceSet)
        {
            return new Domain.ResourceSet
            {
                IconUri = resourceSet.IconUri,
                Name = resourceSet.Name,
                Scopes = GetList(resourceSet.Scopes),
                Id = resourceSet.Id,
                Type = resourceSet.Type,
                Uri = resourceSet.Uri,
                AuthorizationPolicyId = resourceSet.PolicyId
            };
        }

        public static Domain.Scope ToDomain(this Model.Scope scope)
        {
            return new Domain.Scope
            {
                Id = scope.Id,
                Name = scope.Name,
                IconUri = scope.IconUri
            };
        }

        public static Domain.Ticket ToDomain(this Model.Ticket ticket)
        {
            return new Domain.Ticket
            {
                Id = ticket.Id,
                Scopes = GetList(ticket.Scopes),
                ClientId = ticket.ClientId,
                ExpirationDateTime = ticket.ExpirationDateTime,
                ResourceSetId = ticket.ResourceSetId
            };
        }

        public static Domain.Rpt ToDomain(this Model.Rpt rpt)
        {
            return new Domain.Rpt
            {
                TicketId = rpt.TicketId,
                ExpirationDateTime = rpt.ExpirationDateTime,
                ResourceSetId = rpt.ResourceSetId,
                Value = rpt.Value
            };
        }

        public static Domain.Policy ToDomain(this Model.Policy policy)
        {
            var resourceSetIds = new List<string>();
            if (policy.ResourceSets != null &&
                policy.ResourceSets.Any())
            {
                resourceSetIds = policy.ResourceSets.Select(p => p.Id).ToList();
            }

            return new Domain.Policy
            {
                Id = policy.Id,
                IsCustom = policy.IsCustom,
                Script = policy.Script,
                IsResourceOwnerConsentNeeded = policy.IsResourceOwnerConsentNeeded,
                ClientIdsAllowed = GetList(policy.ClientIdsAllowed),
                Scopes = GetList(policy.Scopes),
                ResourceSetIds = resourceSetIds
            };
        }

        #endregion

        #region To Models

        public static Model.ResourceSet ToModel(this Domain.ResourceSet resourceSet)
        {
            return new Model.ResourceSet
            {
                IconUri = resourceSet.IconUri,
                Name = resourceSet.Name,
                Scopes = GetConcatenatedList(resourceSet.Scopes),
                Id = resourceSet.Id,
                Type = resourceSet.Type,
                Uri = resourceSet.Uri,
                PolicyId = resourceSet.AuthorizationPolicyId
            };
        }

        public static Model.Scope ToModel(this Domain.Scope scope)
        {
            return new Model.Scope
            {
                Id = scope.Id,
                Name = scope.Name,
                IconUri = scope.IconUri
            };
        }

        public static Model.Ticket ToModel(this Domain.Ticket ticket)
        {
            return new Model.Ticket
            {
                Id = ticket.Id,
                Scopes = GetConcatenatedList(ticket.Scopes),
                ExpirationDateTime = ticket.ExpirationDateTime,
                ClientId = ticket.ClientId,
                ResourceSetId = ticket.ResourceSetId
            };
        }

        public static Model.Rpt ToModel(this Domain.Rpt rpt)
        {
            return new Model.Rpt
            {
                TicketId = rpt.TicketId,
                ExpirationDateTime = rpt.ExpirationDateTime,
                ResourceSetId = rpt.ResourceSetId,
                Value = rpt.Value
            };
        }

        public static Model.Policy ToModel(this Domain.Policy policy)
        {
            return new Model.Policy
            {
                Id = policy.Id,
                IsCustom = policy.IsCustom,
                Script = policy.Script,
                IsResourceOwnerConsentNeeded = policy.IsResourceOwnerConsentNeeded,
                ClientIdsAllowed = GetConcatenatedList(policy.ClientIdsAllowed),
                Scopes = GetConcatenatedList(policy.Scopes)
            };
        }

        #endregion

        #region Private methods

        private static List<string> GetList(string concatenatedList)
        {

            var scopes = new List<string>();
            if (!string.IsNullOrEmpty(concatenatedList))
            {
                scopes = concatenatedList.Split(',').ToList();
            }

            return scopes;
        }

        private static string GetConcatenatedList(List<string> list)
        {
            var concatenatedList = string.Empty;
            if (list != null &&
                list.Any())
            {
                concatenatedList = string.Join(",", list);
            }

            return concatenatedList;
        }

        #endregion
    }
}
