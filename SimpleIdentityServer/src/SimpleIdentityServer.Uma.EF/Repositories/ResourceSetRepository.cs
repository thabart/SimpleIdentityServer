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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.EF.Repositories
{
    internal class ResourceSetRepository : IResourceSetRepository
    {
        private readonly SimpleIdServerUmaContext _context;

        public ResourceSetRepository(SimpleIdServerUmaContext context)
        {
            _context = context;
        }

        public async Task<bool> Insert(ResourceSet resourceSet)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                { 
                    _context.ResourceSets.Add(resourceSet.ToModel());
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

        public async Task<ResourceSet> Get(string id)
        {
            try
            {
                var resourceSet = await _context.ResourceSets
                    .Include(r => r.PolicyResources)
                    .FirstOrDefaultAsync(r => r.Id == id)
                    .ConfigureAwait(false);
                return resourceSet == null ? null : resourceSet.ToDomain();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> Update(ResourceSet resourceSet)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var record = await _context.ResourceSets.FirstOrDefaultAsync(r => r.Id == resourceSet.Id).ConfigureAwait(false);
                    if (record == null)
                    {
                        return false;
                    }
                    
                    record.Name = resourceSet.Name;
                    record.Scopes = MappingExtensions.GetConcatenatedList(resourceSet.Scopes);
                    record.Type = resourceSet.Type;
                    record.Uri = resourceSet.Uri;
                    record.IconUri = resourceSet.IconUri;

                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<ICollection<ResourceSet>> GetAll()
        {
            try
            {
                return await _context.ResourceSets.Select(r => r.ToDomain()).ToListAsync().ConfigureAwait(false);
            }
            catch
            {
                return null;
            }            
        }

        public async Task<bool> Delete(string id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var record = await _context.ResourceSets
                        .Include(r => r.Rpts)
                        .Include(r => r.Tickets)
                        .FirstOrDefaultAsync(r => r.Id == id)
                        .ConfigureAwait(false);
                    if (record == null)
                    {
                        return false;
                    }

                    _context.ResourceSets.Remove(record);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public async Task<IEnumerable<ResourceSet>> Get(IEnumerable<string> ids)
        {
            try
            {
                return await _context.ResourceSets
                    .Include(r => r.PolicyResources)
                    .Where(r => ids.Contains(r.Id))
                    .Select(r => r.ToDomain())
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}