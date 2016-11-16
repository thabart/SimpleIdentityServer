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
using SimpleIdentityServer.DataAccess.SqlServer.Models;
using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class ConsentRepository : IConsentRepository
    {
        private readonly SimpleIdentityServerContext _context;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IManagerEventSource _managerEventSource;

        public ConsentRepository(
            SimpleIdentityServerContext context,
            IResourceOwnerRepository resourceOwnerRepository,
            IManagerEventSource managerEventSource)
        {
            _context = context;
            _resourceOwnerRepository = resourceOwnerRepository;
            _managerEventSource = managerEventSource;
        }

        public List<Core.Models.Consent> GetConsentsForGivenUser(string subject)
        {
            var resourceOwner = _resourceOwnerRepository.GetByUniqueClaim(subject);
            if (resourceOwner == null)
            {
                return new List<Core.Models.Consent>();
            }

            var consents = _context.Consents
                .Include(c => c.Client)
                .Include(c => c.ResourceOwner)
                .Where(c => c.ResourceOwner.Id == resourceOwner.Id)
                .ToList();

            foreach(var consent in consents)
            {
                var consentClaims = _context.ConsentClaims
                   .Include(c => c.Claim)
                   .Where(c => c.ConsentId == consent.Id)
                   .ToList();
                var consentScopes = _context.ConsentScopes
                    .Include(c => c.Scope)
                    .Where(c => c.ConsentId == consent.Id)
                    .ToList();
                consent.ConsentClaims = consentClaims;
                consent.ConsentScopes = consentScopes;
            }

            return consents.Select(c => c.ToDomain()).ToList();
        }

        public Core.Models.Consent InsertConsent(Core.Models.Consent record)
        {
            Core.Models.Consent result = null;
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var clientId = record.Client.ClientId;
                    var resourceOwnerId = record.ResourceOwner.Id;
                    var client = _context.Clients.FirstOrDefault(c => c.ClientId == clientId);
                    var resourceOwner = _context.ResourceOwners.FirstOrDefault(r => r.Id == resourceOwnerId);
                    var assignedClaims = new List<ConsentClaim>();
                    var assignedScopes = new List<ConsentScope>();
                    if (record.Claims != null)
                    {
                        var claimCodes = record.Claims;
                        var codes = _context.Claims.Where(c => claimCodes.Contains(c.Code)).Select(c => c.Code).ToList();
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
                        var names = _context.Scopes.Where(s => scopeNames.Contains(s.Name)).Select(s => s.Name).ToList();
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
                    _context.SaveChanges();
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

        public bool DeleteConsent(Core.Models.Consent record)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var identifier = int.Parse(record.Id);
                    var consent = _context.Consents.FirstOrDefault(c => c.Id == identifier);
                    if (consent == null)
                    {
                        return false;
                    }

                    _context.Consents.Remove(consent);
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
    }
}
