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
using System.Linq;
using System.Collections.Generic;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Logging;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace SimpleIdentityServer.IdentityServer.EF.Repositories
{
    internal sealed class ScopeRepository : IScopeRepository
    {
        #region Fields

        private readonly ConfigurationDbContext _context;

        private readonly IManagerEventSource _managerEventSource;

        #endregion

        #region Constructor

        public ScopeRepository(
            ConfigurationDbContext context,
            IManagerEventSource managerEventSource)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _context = context;
            _managerEventSource = managerEventSource;
        }

        #endregion

        #region Public methods

        public bool DeleteScope(Scope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

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

        public IList<Scope> GetAllScopes()
        {
            return _context.Scopes.Include(s => s.Claims)
                .Select(s => s.ToDomain()).ToList();
        }

        public Scope GetScopeByName(string name)
        {
            var result = _context.Scopes
                .Include(s => s.Claims)
                .FirstOrDefault(s => s.Name == name);
            if (result == null)
            {
                return null;
            }

            return result.ToDomain();
        }

        public bool InsertScope(Scope scope)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var record = new IdentityServer4.EntityFramework.Entities.Scope
                    {
                        Name = scope.Name,
                        Description = scope.Description,
                        ShowInDiscoveryDocument = scope.IsExposed,
                        Type = scope.Type == ScopeType.ProtectedApi ? 1 : 0,
                        DisplayName = scope.Name,
                        Claims = scope.Claims == null ? new List<IdentityServer4.EntityFramework.Entities.ScopeClaim>()
                            : scope.Claims.Select(s => new IdentityServer4.EntityFramework.Entities.ScopeClaim
                            {
                                Name = s
                            }).ToList()
                    };

                    _context.Scopes.Add(record);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                    return false;
                }
            }

            return true;
        }

        public bool UpdateScope(Scope scope)
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

                    connectedScope.Type = scope.Type == ScopeType.ProtectedApi ? 1 : 0;
                    connectedScope.Description = scope.Description;
                    connectedScope.ShowInDiscoveryDocument = scope.IsExposed;
                    connectedScope.Claims = scope.Claims == null ? new List<IdentityServer4.EntityFramework.Entities.ScopeClaim>()
                            : scope.Claims.Select(s => new IdentityServer4.EntityFramework.Entities.ScopeClaim
                            {
                                Name = s
                            }).ToList();
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

        #endregion
    }
}
