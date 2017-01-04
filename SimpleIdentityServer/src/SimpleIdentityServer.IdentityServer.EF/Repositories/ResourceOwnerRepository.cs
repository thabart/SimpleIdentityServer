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
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.IdentityServer.EF.DbContexts;
using SimpleIdentityServer.Logging;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Claims;

namespace SimpleIdentityServer.IdentityServer.EF.Repositories
{
    public class ResourceOwnerRepository : IResourceOwnerRepository
    {
        private readonly UserDbContext _context;
        private readonly IManagerEventSource _managerEventSource;

        public ResourceOwnerRepository(
            UserDbContext context,
            IManagerEventSource managerEventSource)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (managerEventSource == null)
            {
                throw new ArgumentNullException(nameof(managerEventSource));
            }

            _context = context;
            _managerEventSource = managerEventSource;
        }

        public async Task<bool> DeleteAsync(string subject)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var user = await _context.Users.Include(u => u.Claims)
                        .FirstOrDefaultAsync(u => u.Subject == subject).ConfigureAwait(false);
                    if (user == null)
                    {
                        return false;
                    }

                    _context.Users.Remove(user);
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

        public async Task<ICollection<ResourceOwner>>  GetAllAsync()
        {
            return await _context.Users.Select(u => u.ToDomain()).ToListAsync().ConfigureAwait(false);
        }

        public async Task<ResourceOwner> GetAsync(string subject)
        {
            var user = await _context.Users.Include(u => u.Claims).FirstOrDefaultAsync(r => r.Subject == subject).ConfigureAwait(false);
            if (user == null)
            {
                return null;
            }

            return user.ToDomain();
        }

        public async Task<ResourceOwner> GetAsync(string userName, string hashedPassword)
        {            
            var user = await _context.Users.Include(u => u.Claims).FirstOrDefaultAsync(r => r.Username == userName && r.Password == hashedPassword).ConfigureAwait(false);
            if (user == null)
            {
                return null;
            }
            
            return user.ToDomain();
        }

        public async Task<ICollection<ResourceOwner>> GetAsync(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                return new List<ResourceOwner>();
            }

            return await _context.Users
                .Include(r => r.Claims)
                .Where(r => claims.All(c => r.Claims.Any(sc => sc.Value == c.Value && sc.Key == c.Type)))
                .Select(u => u.ToDomain())
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<bool> Insert(ResourceOwner resourceOwner)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    _context.Users.Add(resourceOwner.ToEntity());
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

        public Task<bool> InsertAsync(ResourceOwner resourceOwner)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateAsync(ResourceOwner resourceOwner)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var user = resourceOwner.ToEntity();
                    var record = await _context.Users
                       .Include(r => r.Claims)
                       .FirstOrDefaultAsync(r => r.Subject == user.Subject)
                       .ConfigureAwait(false);
                    if (record == null)
                    {
                        return false;
                    }

                    record.Username = user.Username;
                    record.Password = user.Password;
                    record.IsLocalAccount = resourceOwner.IsLocalAccount;
                    record.Claims = user.Claims;
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
    }
}
