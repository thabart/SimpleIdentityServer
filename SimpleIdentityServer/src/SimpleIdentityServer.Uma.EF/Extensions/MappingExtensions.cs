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
            var scopes = new List<string>();
            if (!string.IsNullOrEmpty(resourceSet.Scopes))
            {
                scopes = resourceSet.Scopes.Split(',').ToList();
            }

            return new Domain.ResourceSet
            {
                IconUri = resourceSet.IconUri,
                Name = resourceSet.Name,
                Scopes = scopes,
                Id = resourceSet.Id,
                Type = resourceSet.Type,
                Uri = resourceSet.Uri
            };
        }

        #endregion

        #region To Models

        public static Model.ResourceSet ToModel(this Domain.ResourceSet resourceSet)
        {
            var scopes = string.Empty;
            if (resourceSet.Scopes != null &&
                resourceSet.Scopes.Any())
            {
                scopes = string.Join(",", resourceSet.Scopes);
            }

            return new Model.ResourceSet
            {
                IconUri = resourceSet.IconUri,
                Name = resourceSet.Name,
                Scopes = scopes,
                Id = resourceSet.Id,
                Type = resourceSet.Type,
                Uri = resourceSet.Uri
            };
        }

        #endregion
    }
}
