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

using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.EF.Extensions;
using System;
using System.Linq;

namespace SimpleIdentityServer.Uma.EF.Repositories
{
    internal class ResourceSetRepository : IResourceSetRepository
    {
        private readonly SimpleIdServerUmaContext _simpeIdServerUmaContext;

        #region Constructor

        public ResourceSetRepository(SimpleIdServerUmaContext simpleIdServerUmaContext)
        {
            _simpeIdServerUmaContext = simpleIdServerUmaContext;
        }

        #endregion

        #region Public methods

        public ResourceSet Insert(ResourceSet resourceSet)
        {
            try
            {
                var model = resourceSet.ToModel();
                model.Id = Guid.NewGuid().ToString();
                _simpeIdServerUmaContext.Add(model);
                _simpeIdServerUmaContext.SaveChanges();
                return model.ToDomain();
            }
            catch(Exception)
            {
                return null;
            }
        }

        public ResourceSet GetResourceSetById(string id)
        {
            var resourceSet = _simpeIdServerUmaContext.ResourceSets.FirstOrDefault(r => r.Id == id);
            if (resourceSet == null)
            {
                return null;
            }

            return resourceSet.ToDomain();
        }

        public ResourceSet UpdateResource(ResourceSet resourceSet)
        {
            var record = _simpeIdServerUmaContext.ResourceSets.FirstOrDefault(r => r.Id == resourceSet.Id);
            if (record == null)
            {
                return null;
            }

            var rs = resourceSet.ToModel();
            record.Name = rs.Name;
            record.Scopes = rs.Scopes;
            record.Type = rs.Type;
            record.Uri = rs.Uri;
            record.IconUri = rs.IconUri;

            _simpeIdServerUmaContext.SaveChanges();
            return record.ToDomain();
        }

        #endregion
    }
}
