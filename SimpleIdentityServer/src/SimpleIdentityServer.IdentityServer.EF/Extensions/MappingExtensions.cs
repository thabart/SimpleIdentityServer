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

using SimpleIdentityServer.Core.Models;
using System.Linq;

namespace SimpleIdentityServer.IdentityServer.EF
{
    internal static class MappingExtensions
    {
        #region To domain

        public static Scope ToDomain(this IdentityServer4.EntityFramework.Entities.Scope scope)
        {
            var standardScopeNames = IdentityServer4.Models.StandardScopes.All.Select(s => s.Name);
            var record = new Scope
            {
                Name = scope.Name,
                Description = scope.Description,
                IsExposed = scope.ShowInDiscoveryDocument,
                IsDisplayedInConsent = true
            };
            record.IsOpenIdScope = standardScopeNames.Contains(record.Name);
            record.Type = scope.Type == (int)IdentityServer4.Models.ScopeType.Identity ?
                ScopeType.ResourceOwner : ScopeType.ProtectedApi;
            if (scope.Claims != null && scope.Claims.Any())
            {
                record.Claims = scope.Claims.Select(c => c.Name).ToList();
            }

            return record;
        }

        #endregion
    }
}
