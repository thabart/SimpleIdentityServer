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
using System.Threading.Tasks;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class ConsentRepository : IConsentRepository
    {
        private readonly SimpleIdentityServerContext _context;
        private readonly IManagerEventSource _managerEventSource;

        public ConsentRepository(SimpleIdentityServerContext context, IManagerEventSource managerEventSource)
        {
            _context = context;
            _managerEventSource = managerEventSource;
        }
        
        public async Task<IEnumerable<Core.Models.Consent>> GetConsentsForGivenUserAsync(string subject)
        {
            var resourceOwnerClaim = await _context.ResourceOwnerClaims
                .Include(r => r.Claim)
                .Include(r => r.ResourceOwner).ThenInclude(r => r.Consents).ThenInclude(r => r.ConsentClaims)
                .Include(r => r.ResourceOwner).ThenInclude(r => r.Consents).ThenInclude(r => r.ConsentScopes)
                .Include(r => r.ResourceOwner).ThenInclude(r => r.Consents).ThenInclude(r => r.Client)
                .Where(r => r.Claim != null && r.Claim.IsIdentifier == true && r.Value == subject)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            if (resourceOwnerClaim == null)
            {
                return null;
            }

            return resourceOwnerClaim.ResourceOwner.Consents == null ? new Core.Models.Consent[0] : resourceOwnerClaim.ResourceOwner.Consents.Select(c => c.ToDomain());
        }

        public async Task<Core.Models.Consent> InsertAsync(Core.Models.Consent record)
        {
            Core.Models.Consent result = null;
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var clientId = record.Client.ClientId;
                    var resourceOwnerId = record.ResourceOwner.Id;
                    var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId).ConfigureAwait(false);
                    var resourceOwner = await _context.ResourceOwners.FirstOrDefaultAsync(r => r.Id == resourceOwnerId).ConfigureAwait(false);
                    var assignedClaims = new List<ConsentClaim>();
                    var assignedScopes = new List<ConsentScope>();
                    if (record.Claims != null)
                    {
                        var claimCodes = record.Claims;
                        var codes = await _context.Claims.Where(c => claimCodes.Contains(c.Code)).Select(c => c.Code).ToListAsync().ConfigureAwait(false);
                        foreach (var code in codes)
                        {
                            assignedClaims.Add(new ConsentClaim
                            {
                                ClaimCode = code
                            });
                        }
                    }

                    if (record.GrantedScopes != null)
                    {
                        var scopeNames = record.GrantedScopes.Select(g => g.Name);
                        var names = await _context.Scopes.Where(s => scopeNames.Contains(s.Name)).Select(s => s.Name).ToListAsync().ConfigureAwait(false);
                        foreach (var name in names)
                        {
                            assignedScopes.Add(new ConsentScope
                            {
                                ScopeName = name
                            });
                        }
                    }

                    var newConsent = new Consent
                    {
                        Client = client,
                        ResourceOwner = resourceOwner,
                        ConsentClaims = assignedClaims,
                        ConsentScopes = assignedScopes
                    };

                    var insertedConsent = _context.Consents.Add(newConsent);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    result = insertedConsent.Entity.ToDomain();
                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                }
            }

            return result;
        }

        public async Task<bool> DeleteAsync(Core.Models.Consent record)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var identifier = int.Parse(record.Id);
                    var consent = await _context.Consents.FirstOrDefaultAsync(c => c.Id == identifier).ConfigureAwait(false);
                    if (consent == null)
                    {
                        return false;
                    }

                    _context.Consents.Remove(consent);
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
    }
}
