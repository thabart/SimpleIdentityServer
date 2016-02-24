﻿using System;
using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using SimpleIdentityServer.DataAccess.SqlServer.Models;
using Microsoft.Data.Entity;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class ConsentRepository : IConsentRepository
    {
        private readonly SimpleIdentityServerContext _context;
        
        public ConsentRepository(SimpleIdentityServerContext context) {
            _context = context;
        }
        
        public List<Core.Models.Consent> GetConsentsForGivenUser(string subject)
        {
                var consents = _context.Consents.Include(c => c.Claims)
                    .Include(c => c.GrantedScopes)
                    .Include(c => c.Client)
                    .Include(c => c.ResourceOwner)
                    .Where(c => c.ResourceOwner.Id == subject)
                    .ToList();
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
                        var assignedClaims = new List<Claim>();
                        var assignedScopes = new List<Scope>();
                        if (record.Claims != null)
                        {
                            var claimCodes = record.Claims;
                            assignedClaims = _context.Claims.Where(c => claimCodes.Contains(c.Code)).ToList();
                        }

                        if (record.GrantedScopes != null)
                        {
                            var scopeNames = record.GrantedScopes.Select(g => g.Name);
                            assignedScopes = _context.Scopes.Where(s => scopeNames.Contains(s.Name)).ToList();
                        }

                        var newConsent = new Consent
                        {
                            Client = client,
                            ResourceOwner = resourceOwner,
                            Claims = assignedClaims,
                            GrantedScopes = assignedScopes
                        };

                        var insertedConsent = _context.Consents.Add(newConsent);
                        _context.SaveChanges();
                        transaction.Commit();
                        result = insertedConsent.Entity.ToDomain();
                    }
                    catch (Exception)
                    {
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
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

            return true;
        }
    }
}
