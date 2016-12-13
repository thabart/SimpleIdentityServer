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

using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.EF.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.EF.Repositories
{
    internal class ScopeRepository : IScopeRepository
    {
        private readonly SimpleIdServerUmaContext _context;

        public ScopeRepository(SimpleIdServerUmaContext context)
        {
            _context = context;
        }

        public async Task<bool> Delete(string id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var scope = await _context.Scopes.FirstOrDefaultAsync(s => s.Id == id).ConfigureAwait(false);
                    if (scope == null)
                    {
                        return false;
                    }

                    _context.Scopes.Remove(scope);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<ICollection<Scope>> GetAll()
        {
            return await _context.Scopes.Select(s => s.ToDomain()).ToListAsync().ConfigureAwait(false);
        }

        public async Task<Scope> Get(string id)
        {
            var scope = await _context.Scopes.FirstOrDefaultAsync(s => s.Id == id);
            return scope == null ? null : scope.ToDomain();
        }

        public async Task<bool> Insert(Scope scope)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    _context.Scopes.Add(scope.ToModel());
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<bool> Update(Scope scope)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var record = await _context.Scopes.FirstOrDefaultAsync(s => s.Id == scope.Id);
                    record.Name = scope.Name;
                    record.IconUri = scope.IconUri;
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }
    }
}