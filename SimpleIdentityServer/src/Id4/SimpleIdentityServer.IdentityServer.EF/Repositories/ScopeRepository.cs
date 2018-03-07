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

using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.IdentityServer.EF.Repositories
{
    internal sealed class ScopeRepository : IScopeRepository
    {
        private readonly ConfigurationDbContext _context;
        private readonly IManagerEventSource _managerEventSource;

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

        public async Task<bool> DeleteAsync(Scope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var connectedScope = await _context.Scopes
                        .FirstOrDefaultAsync(c => c.Name == scope.Name)
                        .ConfigureAwait(false);
                    if (connectedScope == null)
                    {
                        return false;
                    }

                    _context.Scopes.Remove(connectedScope);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
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

        public async Task<ICollection<Scope>> GetAllAsync()
        {
            return await _context.Scopes.Include(s => s.Claims)
                .Select(s => s.ToDomain()).ToListAsync().ConfigureAwait(false);
        }

        public async Task<Scope> GetAsync(string name)
        {
            var result = await _context.Scopes
                .Include(s => s.Claims)
                .FirstOrDefaultAsync(s => s.Name == name)
                .ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            return result.ToDomain();
        }

        public async Task<bool> InsertAsync(Scope scope)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
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
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return true;
                }
                catch(Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<bool> UpdateAsync(Scope scope)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var connectedScope = await _context.Scopes
                        .FirstOrDefaultAsync(c => c.Name == scope.Name)
                        .ConfigureAwait(false);
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
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<ICollection<Scope>> SearchByNamesAsync(IEnumerable<string> names)
        {
            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            return await _context.Scopes
                .Include(s => s.Claims)
                .Where(s => names.Contains(s.Name))
                .Select(s => s.ToDomain())
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
