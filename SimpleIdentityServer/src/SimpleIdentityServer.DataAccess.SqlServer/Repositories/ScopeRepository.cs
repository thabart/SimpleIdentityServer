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

using System;
using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using Domains = SimpleIdentityServer.Core.Models;
using Microsoft.Data.Entity;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class ScopeRepository : IScopeRepository
    {
         private readonly SimpleIdentityServerContext _context;
        
        public ScopeRepository(SimpleIdentityServerContext context) {
            _context = context;
        }

        public bool InsertScope(Domains.Scope scope)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var record = new Models.Scope
                    {
                        Name = scope.Name,
                        Description = scope.Description,
                        IsDisplayedInConsent = scope.IsDisplayedInConsent,
                        IsExposed = scope.IsExposed,
                        IsOpenIdScope = scope.IsOpenIdScope,
                        Type = (Models.ScopeType)scope.Type
                    };

                    if (scope.Claims != null &&
                        scope.Claims.Any())
                    {
                        record.ScopeClaims = scope.Claims.Select(c => new Models.ScopeClaim { ClaimCode = c }).ToList();
                    }

                    _context.Scopes.Add(record);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }

            return true;
        }

        public Domains.Scope GetScopeByName(string name)
        {
            var result = _context.Scopes
                .FirstOrDefault(s => s.Name == name);
            if (result == null)
            {
                return null;
            }


            result.ScopeClaims = _context.ScopeClaims
                .Include(c => c.Claim)
                .Where(c => c.ScopeName == name)
                .ToList();
            return result.ToDomain();
        }

        public IList<Domains.Scope> GetAllScopes()
        {
            var result = _context.Scopes.ToList();
            return result.Select(r => r.ToDomain()).ToList();
        }
    }
}
