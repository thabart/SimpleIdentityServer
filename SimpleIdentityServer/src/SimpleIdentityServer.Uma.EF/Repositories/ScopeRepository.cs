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
using System;
using System.Linq;
using System.Collections.Generic;
using SimpleIdentityServer.Uma.EF.Extensions;
using Model = SimpleIdentityServer.Uma.EF.Models;

namespace SimpleIdentityServer.Uma.EF.Repositories
{
    internal class ScopeRepository : IScopeRepository
    {
        private readonly SimpleIdServerUmaContext _simpleIdServerUmaContext;

        #region Constructor

        public ScopeRepository(SimpleIdServerUmaContext simpleIdServerUmaContext)
        {
            _simpleIdServerUmaContext = simpleIdServerUmaContext;
        }

        #endregion

        #region Public methods

        public bool DeleteScope(string id)
        {   
            var scope = _simpleIdServerUmaContext.Scopes.FirstOrDefault(s => s.Id == id);
            if (scope == null)
            {
                return false;
            }

            _simpleIdServerUmaContext.Scopes.Remove(scope);
            _simpleIdServerUmaContext.SaveChanges();
            return true;
        }

        public List<Scope> GetAll()
        {   
            var scopes = _simpleIdServerUmaContext.Scopes;
            return scopes.Select(s => s.ToDomain()).ToList();
        }

        public Scope GetScope(string id)
        {
            var scope = _simpleIdServerUmaContext.Scopes.FirstOrDefault(s => s.Id == id);
            if (scope == null)
            {
                return null;
            }

            return scope.ToDomain();
        }

        public Scope InsertScope(Scope scope)
        {
            var record = scope.ToModel();
            _simpleIdServerUmaContext.Scopes.Add(record);
            _simpleIdServerUmaContext.SaveChanges();
            return scope;
        }

        public Scope UpdateScope(Scope scope)
        {
            var record = _simpleIdServerUmaContext.Scopes.FirstOrDefault(s => s.Id == scope.Id);
            if (record == null)
            {
                return null;
            }
            
            record.Name = scope.Name;
            record.IconUri = scope.IconUri;
            _simpleIdServerUmaContext.SaveChanges();
            return scope;
        }

        #endregion
    }
}
