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
using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Common.Results;
using SimpleIdentityServer.EF.Extensions;
using SimpleIdentityServer.EF.Models;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domains = SimpleIdentityServer.Core.Common.Models;

namespace SimpleIdentityServer.EF.Repositories
{
    public sealed class ResourceOwnerRepository : IResourceOwnerRepository
    {
        private readonly SimpleIdentityServerContext _context;
        private readonly ITechnicalEventSource _managerEventSource;

        public ResourceOwnerRepository(
            SimpleIdentityServerContext context,
            ITechnicalEventSource managerEventSource)
        {
            _context = context;
            _managerEventSource = managerEventSource;
        }

        public async Task<Domains.ResourceOwner> GetResourceOwnerByClaim(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            try
            {
                var result = await _context.ResourceOwners.Include(r => r.Claims)
                    .FirstOrDefaultAsync(r => r.Claims.Any(c => c.ClaimCode == key && c.Value == value))
                    .ConfigureAwait(false);
                if (result == null)
                {
                    return null;
                }

                return result.ToDomain();
            }
            catch(Exception ex)
            {
                _managerEventSource.Failure(ex);
                return null;
            }
        }
        
        public async Task<Domains.ResourceOwner> GetAsync(string id)
        {
            try
            {
                var claimIdentifier = await _context.Claims.FirstOrDefaultAsync(c => c.IsIdentifier).ConfigureAwait(false);
                if (claimIdentifier == null)
                {
                    throw new InvalidOperationException("no claim can be used to uniquely identified the resource owner");
                }

                var result = await _context.ResourceOwners
                    .Include(r => r.Claims)
                    .FirstOrDefaultAsync(r => r.Claims.Any(c => c.ClaimCode == claimIdentifier.Code && c.Value == id))
                    .ConfigureAwait(false);
                if (result == null)
                {
                    return null;
                }

                return result.ToDomain();
            }
            catch (Exception ex)
            {
                _managerEventSource.Failure(ex);
                return null;
            }
        }

        public async Task<Domains.ResourceOwner> GetAsync(string id, string password)
        {
            try
            {
                var claimIdentifier = await _context.Claims.FirstOrDefaultAsync(c => c.IsIdentifier).ConfigureAwait(false);
                if (claimIdentifier == null)
                {
                    throw new InvalidOperationException("no claim can be used to uniquely identified the resource owner");
                }

                var result = await _context.ResourceOwners
                    .Include(r => r.Claims)
                    .FirstOrDefaultAsync(r => r.Claims.Any(c => c.ClaimCode == claimIdentifier.Code && c.Value == id) && r.Password == password)
                    .ConfigureAwait(false);
                if (result == null)
                {
                    return null;
                }

                return result.ToDomain();
            }
            catch (Exception ex)
            {
                _managerEventSource.Failure(ex);
                return null;
            }
        }

        public async Task<ICollection<Domains.ResourceOwner>> GetAsync(IEnumerable<System.Security.Claims.Claim> claims)
        {
            if (claims == null)
            {
                return new List<Domains.ResourceOwner>();
            }

            return await _context.ResourceOwners
                .Include(r => r.Claims)
                .Where(r => claims.All(c => r.Claims.Any(sc => sc.Value == c.Value && sc.ClaimCode == c.Type)))
                .Select(u => u.ToDomain())
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<ICollection<Domains.ResourceOwner>> GetAllAsync()
        {
            return await _context.ResourceOwners.Select(u => u.ToDomain()).ToListAsync().ConfigureAwait(false);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var record = await _context.ResourceOwners
                   .Include(r => r.Claims)
                   .Include(r => r.Consents)
                   .FirstOrDefaultAsync(r => r.Id == id).ConfigureAwait(false);
                if (record == null)
                {
                    return false;
                }

                _context.ResourceOwners.Remove(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _managerEventSource.Failure(ex);
                return false;
            }
        }

        public async Task<bool> InsertAsync(Domains.ResourceOwner resourceOwner)
        {
            try
            {
                var user = new ResourceOwner
                {
                    Id = resourceOwner.Id,
                    Password = resourceOwner.Password,
                    IsLocalAccount = resourceOwner.IsLocalAccount,
                    TwoFactorAuthentication = resourceOwner.TwoFactorAuthentication,
                    Claims = new List<ResourceOwnerClaim>(),
                    CreateDateTime = DateTime.UtcNow,
                    UpdateDateTime = DateTime.UtcNow
                };

                if (resourceOwner.Claims != null)
                {
                    foreach (var claim in resourceOwner.Claims)
                    {
                        user.Claims.Add(new ResourceOwnerClaim
                        {
                            Id = Guid.NewGuid().ToString(),
                            ResourceOwnerId = user.Id,
                            ClaimCode = claim.Type,
                            Value = claim.Value
                        });
                    }
                }

                _context.ResourceOwners.Add(user);
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _managerEventSource.Failure(ex);
                return false;
            }

            return true;
        }

        public async Task<bool> UpdateAsync(Domains.ResourceOwner resourceOwner)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var record = await _context.ResourceOwners
                        .Include(r => r.Claims)
                        .FirstOrDefaultAsync(r => r.Id == resourceOwner.Id).ConfigureAwait(false);
                    if (record == null)
                    {
                        return false;
                    }

                    record.Password = resourceOwner.Password;
                    record.IsLocalAccount = resourceOwner.IsLocalAccount;
                    record.TwoFactorAuthentication = resourceOwner.TwoFactorAuthentication;
                    record.UpdateDateTime = DateTime.UtcNow;
                    record.Claims = new List<ResourceOwnerClaim>();
                    _context.ResourceOwnerClaims.RemoveRange(record.Claims);
                    if (resourceOwner.Claims != null)
                    {
                        foreach (var claim in resourceOwner.Claims)
                        {
                            record.Claims.Add(new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ResourceOwnerId = record.Id,
                                ClaimCode = claim.Type,
                                Value = claim.Value
                            });
                        }
                    }

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

        public async Task<SearchResourceOwnerResult> Search(SearchResourceOwnerParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            try
            {
                var claimIdentifier = await _context.Claims.FirstOrDefaultAsync(c => c.IsIdentifier).ConfigureAwait(false);
                if (claimIdentifier == null)
                {
                    throw new InvalidOperationException("no claim can be used to uniquely identified the resource owner");
                }

                IQueryable<Models.ResourceOwner> result = _context.ResourceOwners
                    .Include(r => r.Claims);

                if (parameter.Subjects != null)
                {                    
                    result = result.Where(r => r.Claims.Any(c => c.ClaimCode == claimIdentifier.Code 
                        && parameter.Subjects.Any(s => c.Value.Contains(s))));
                }

                if (result == null)
                {
                    return null;
                }

                var nbResult = await result.CountAsync().ConfigureAwait(false);
                if (parameter.Order != null)
                {
                    switch (parameter.Order.Target)
                    {
                        case "update_datetime":
                            switch (parameter.Order.Type)
                            {
                                case OrderTypes.Asc:
                                    result = result.OrderBy(c => c.UpdateDateTime);
                                    break;
                                case OrderTypes.Desc:
                                    result = result.OrderByDescending(c => c.UpdateDateTime);
                                    break;
                            }
                            break;
                    }
                }
                else
                {
                    result = result.OrderByDescending(c => c.UpdateDateTime);
                }

                if (parameter.IsPagingEnabled)
                {
                    result = result.Skip(parameter.StartIndex).Take(parameter.Count);
                }

                return new SearchResourceOwnerResult
                {
                    Content = result.Select(r => r.ToDomain()),
                    StartIndex = parameter.StartIndex,
                    TotalResults = nbResult
                };
            }
            catch (Exception ex)
            {
                _managerEventSource.Failure(ex);
                return null;
            }
        }
    }
}
