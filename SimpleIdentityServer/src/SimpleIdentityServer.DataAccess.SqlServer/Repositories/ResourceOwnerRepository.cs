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
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using SimpleIdentityServer.DataAccess.SqlServer.Models;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Domains = SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class ResourceOwnerRepository : IResourceOwnerRepository
    {
        private readonly SimpleIdentityServerContext _context;
        private readonly IManagerEventSource _managerEventSource;

        public ResourceOwnerRepository(
            SimpleIdentityServerContext context,
            IManagerEventSource managerEventSource)
        {
            _context = context;
            _managerEventSource = managerEventSource;
        }

        public bool Insert(Domains.ResourceOwner resourceOwner)
        {
            try
            {
                var user = new ResourceOwner
                {
                    Id = resourceOwner.Id,
                    Password = resourceOwner.Password,
                    IsLocalAccount = resourceOwner.IsLocalAccount,
                    TwoFactorAuthentication = (int)resourceOwner.TwoFactorAuthentication,
                    Claims = new List<ResourceOwnerClaim>()
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
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _managerEventSource.Failure(ex);
                return false;
            }

            return true;
        }

        public bool Update(Domains.ResourceOwner resourceOwner)
        {
            try
            {
                var record = _context.ResourceOwners
                    .Include(r => r.Claims)
                    .FirstOrDefault(r => r.Id == resourceOwner.Id);
                if (record == null)
                {
                    return false;
                }

                record.Password = resourceOwner.Password;
                record.IsLocalAccount = resourceOwner.IsLocalAccount;
                record.TwoFactorAuthentication = (int)resourceOwner.TwoFactorAuthentication;
                record.Claims = new List<ResourceOwnerClaim>();
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

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _managerEventSource.Failure(ex);
                return false;
            }
        }

        public List<Domains.ResourceOwner> GetAll()
        {
            var users = _context.ResourceOwners.ToList();
            return users.Select(u => u.ToDomain()).ToList();
        }

        public List<Domains.ResourceOwner> GetResourceOwners(IEnumerable<System.Security.Claims.Claim> claims)
        {
            if (claims == null)
            {
                return new List<Domains.ResourceOwner>();
            }

            var resourceOwners = _context.ResourceOwners
                .Include(r => r.Claims);
            return _context.ResourceOwners
                .Include(r => r.Claims)
                .Where(r => claims.All(c => r.Claims.Any(sc => sc.Value == c.Value && sc.ClaimCode == c.Type)))
                .Select(u => u.ToDomain())
                .ToList();
        }

        public bool Delete(string id)
        {
            try
            {
                var record = _context.ResourceOwners
                   .Include(r => r.Claims)
                   .Include(r => r.Consents)
                   .FirstOrDefault(r => r.Id == id);
                if (record == null)
                {
                    return false;
                }

                _context.ResourceOwners.Remove(record);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _managerEventSource.Failure(ex);
                return false;
            }
        }

        public Domains.ResourceOwner Get(string id)
        {
            try
            {
                var result = _context.ResourceOwners.Include(r => r.Claims).FirstOrDefault(r => r.Id == id);
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

        public Domains.ResourceOwner Get(string id, string password)
        {
            try
            {
                var result = _context.ResourceOwners.Include(r => r.Claims).FirstOrDefault(r => r.Id == id && r.Password == password);
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
    }
}
