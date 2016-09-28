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
using SimpleIdentityServer.Core.Repositories;
using IdentityServer4.EntityFramework.DbContexts;
using SimpleIdentityServer.Logging;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SimpleIdentityServer.IdentityServer.EF.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly ConfigurationDbContext _context;

        private readonly IManagerEventSource _managerEventSource;

        #region Constructor

        public ClientRepository(
            ConfigurationDbContext context,
            IManagerEventSource managerEventSource)
        {
            _context = context;
            _managerEventSource = managerEventSource;
        }

        #endregion

        public bool DeleteClient(SimpleIdentityServer.Core.Models.Client client)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var result = _context.Clients.FirstOrDefault(c => c.ClientId == client.ClientId);
                    if (result == null)
                    {
                        return false;
                    }

                    _context.Clients.Remove(result);
                    _context.SaveChanges();
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

        public IList<Core.Models.Client> GetAll()
        {
            var clients = _context.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.ClientSecrets)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.AllowedScopes)
                .Include(c => c.IdentityProviderRestrictions)
                .Include(c => c.Claims)
                .Include(c => c.AllowedCorsOrigins)
                .ToList();
            return clients.Select(c => c.ToDomain()).ToList();
        }

        public Core.Models.Client GetClientById(string clientId)
        {
            var client = _context.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.ClientSecrets)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.AllowedScopes)
                .Include(c => c.IdentityProviderRestrictions)
                .Include(c => c.Claims)
                .Include(c => c.AllowedCorsOrigins)
                .FirstOrDefault(c => c.ClientId == clientId);
            if (client == null)
            {
                return null;
            }

            return client.ToDomain();
        }

        public bool InsertClient(Core.Models.Client client)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var record = client.ToEntity();
                    _context.Clients.Add(record);
                    _context.SaveChanges();
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

        public bool UpdateClient(Core.Models.Client client)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var record = client.ToEntity();
                    var result = _context.Clients.FirstOrDefault(c => c.ClientId == record.ClientId);
                    if (result == null)
                    {
                        return false;
                    }

                    result.ClientName = record.ClientName;
                    result.Enabled = record.Enabled;
                    result.RequireClientSecret = record.RequireClientSecret;
                    result.RequireConsent = record.RequireConsent;
                    result.AllowRememberConsent = record.AllowRememberConsent;
                    result.LogoutSessionRequired = record.LogoutSessionRequired;
                    result.IdentityTokenLifetime = record.IdentityTokenLifetime;
                    result.AccessTokenLifetime = record.AccessTokenLifetime;
                    result.AuthorizationCodeLifetime = record.AuthorizationCodeLifetime;
                    result.AbsoluteRefreshTokenLifetime = record.AbsoluteRefreshTokenLifetime;
                    result.SlidingRefreshTokenLifetime = record.SlidingRefreshTokenLifetime;
                    result.RefreshTokenUsage = record.RefreshTokenUsage;
                    result.RefreshTokenExpiration = record.RefreshTokenExpiration;
                    result.EnableLocalLogin = record.EnableLocalLogin;
                    result.PrefixClientClaims = record.PrefixClientClaims;
                    result.LogoUri = record.LogoUri;
                    result.ClientUri = record.ClientUri;
                    result.AllowedScopes = record.AllowedScopes;
                    result.RedirectUris = record.RedirectUris;
                    result.AllowedGrantTypes = record.AllowedGrantTypes;
                    _context.SaveChanges();
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

        public bool RemoveAll()
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Clients.RemoveRange(_context.Clients);
                    _context.SaveChanges();
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
