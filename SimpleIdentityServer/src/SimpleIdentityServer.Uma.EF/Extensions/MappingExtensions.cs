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
                Scopes = GetScopes(resourceSet.Scopes),
                Id = resourceSet.Id,
                Type = resourceSet.Type,
                Uri = resourceSet.Uri
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
                Scopes = GetScopes(ticket.Scopes),
                ClientId = ticket.ClientId,
                ExpirationDateTime = ticket.ExpirationDateTime,
                ResourceSetId = ticket.ResourceSetId
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
                Scopes = GetConcatenatedScopes(resourceSet.Scopes),
                Id = resourceSet.Id,
                Type = resourceSet.Type,
                Uri = resourceSet.Uri
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
                Scopes = GetConcatenatedScopes(ticket.Scopes),
                ExpirationDateTime = ticket.ExpirationDateTime,
                ClientId = ticket.ClientId,
                ResourceSetId = ticket.ResourceSetId
            };
        }

        #endregion

        #region Private methods

        private static List<string> GetScopes(string cpncatenatedScopes)
        {

            var scopes = new List<string>();
            if (!string.IsNullOrEmpty(cpncatenatedScopes))
            {
                scopes = cpncatenatedScopes.Split(',').ToList();
            }

            return scopes;
        }

        private static string GetConcatenatedScopes(List<string> scopes)
        {
            var concatenatedScopes = string.Empty;
            if (scopes != null &&
                scopes.Any())
            {
                concatenatedScopes = string.Join(",", scopes);
            }

            return concatenatedScopes;
        }

        #endregion
    }
}
