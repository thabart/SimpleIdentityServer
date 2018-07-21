﻿#region copyright
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

using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domains = SimpleIdentityServer.Core.Models;

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

        public async Task<Domains.Scope> GetAsync(string name)
        {
            var result = await _context.Scopes
                .Include(s => s.ScopeClaims).ThenInclude(c => c.Claim)
                .FirstOrDefaultAsync(s => s.Name == name)
                .ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            return result.ToDomain();
        }

        public async Task<ICollection<Domains.Scope>> SearchByNamesAsync(IEnumerable<string> names)
        {
            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            return await _context.Scopes
                .Include(s => s.ScopeClaims).ThenInclude(c => c.Claim)
                .Where(s => names.Contains(s.Name))
                .Select(s => s.ToDomain())
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<ICollection<Domains.Scope>> GetAllAsync()
        {
            return await _context.Scopes
                .Include(s => s.ScopeClaims).ThenInclude(c => c.Claim)
                .Select(r => r.ToDomain())
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<bool> InsertAsync(Domains.Scope scope)
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
                        foreach (var type in scope.Claims)
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

        public async Task<bool> DeleteAsync(Domains.Scope scope)
        {
            using (var transaction = _context.Database.BeginTransaction())
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

        public async Task<bool> UpdateAsync(Domains.Scope scope)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var connectedScope = await _context.Scopes
                        .Include(s => s.ScopeClaims)
                        .FirstOrDefaultAsync(c => c.Name == scope.Name)
                        .ConfigureAwait(false);
                    connectedScope.Description = scope.Description;
                    connectedScope.IsOpenIdScope = scope.IsOpenIdScope;
                    connectedScope.IsDisplayedInConsent = scope.IsDisplayedInConsent;
                    connectedScope.IsExposed = scope.IsExposed;
                    connectedScope.Type = (Models.ScopeType)scope.Type;
                    var scopesNotToBeRemoved = new List<string>();
                    if (scope.Claims != null &&
                        scope.Claims.Any())
                    {
                        foreach (var type in scope.Claims)
                        {
                            var rec = _context.Claims.FirstOrDefault(c => c.Code == type);
                            var scopeClaims = connectedScope.ScopeClaims.FirstOrDefault(c => c.ClaimCode == type);
                            if (rec == null)
                            {
                                rec = new Models.Claim { Code = type };
                                _context.Claims.Add(rec);
                            }

                            if (scopeClaims == null)
                            {
                                connectedScope.ScopeClaims.Add(new Models.ScopeClaim
                                {
                                    ClaimCode = rec.Code,
                                    ScopeName = connectedScope.Name
                                });
                            }

                            scopesNotToBeRemoved.Add(type);
                        }
                    }

                    foreach(var scopeClaim in connectedScope.ScopeClaims.Where(s => !scopesNotToBeRemoved.Any(c => c == s.ClaimCode)).ToList())
                    {
                        _context.ScopeClaims.Remove(scopeClaim);
                    }

                    await _context.SaveChangesAsync().ConfigureAwait(false);
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
