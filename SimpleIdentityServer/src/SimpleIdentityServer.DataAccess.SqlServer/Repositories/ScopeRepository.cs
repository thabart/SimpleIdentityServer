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
using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class ScopeRepository : IScopeRepository
    {
        private readonly SimpleIdentityServerContext _context;

        private readonly IManagerEventSource _managerEventSource;

        public ScopeRepository(
            SimpleIdentityServerContext context,
            IManagerEventSource managerEventSource) {
            _context = context;
            _managerEventSource = managerEventSource;
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
                        Type = (Models.ScopeType)scope.Type,
                        ScopeClaims = new List<Models.ScopeClaim>()
                    };

                    if (scope.Claims != null &&
                        scope.Claims.Any())
                    {
                        foreach(var type in scope.Claims)
                        {
                            var rec = _context.Claims.FirstOrDefault(c => c.Code == type);
                            if (rec == null)
                            {
                                rec = new Models.Claim { Code = type };
                                _context.Claims.Add(rec);
                            }

                            record.ScopeClaims.Add(new Models.ScopeClaim { Claim = rec });
                        }
                    }

                    _context.Scopes.Add(record);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
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
            foreach(var scope in result)
            {
                scope.ScopeClaims = _context.ScopeClaims
                .Include(c => c.Claim)
                .Where(c => c.ScopeName == scope.Name)
                .ToList();
            }

            return result.Select(r => r.ToDomain()).ToList();
        }

        public bool DeleteScope(Domains.Scope scope)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var connectedScope = _context.Scopes
                        .FirstOrDefault(c => c.Name == scope.Name);
                    if (connectedScope == null)
                    {
                        return false;
                    }

                    _context.Scopes.Remove(connectedScope);
                    _context.SaveChanges();
                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                    return false;
                }
            }

            return true;
        }

        public bool UpdateScope(Domains.Scope scope)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var connectedScope = _context.Scopes
                        .FirstOrDefault(c => c.Name == scope.Name);
                    connectedScope.Description = scope.Description;
                    connectedScope.IsDisplayedInConsent = scope.IsDisplayedInConsent;
                    connectedScope.IsExposed = scope.IsExposed;
                    _context.SaveChanges();
                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                    return false;
                }

                return true;
            }
        }
    }
}
