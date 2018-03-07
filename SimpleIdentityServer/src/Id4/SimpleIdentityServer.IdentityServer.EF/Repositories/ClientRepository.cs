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

using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.IdentityServer.EF.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly ConfigurationDbContext _context;
        private readonly IManagerEventSource _managerEventSource;

        public ClientRepository(
            ConfigurationDbContext context,
            IManagerEventSource managerEventSource)
        {
            _context = context;
            _managerEventSource = managerEventSource;
        }

        public async Task<bool> DeleteAsync(Core.Models.Client client)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var result = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == client.ClientId).ConfigureAwait(false);
                    if (result == null)
                    {
                        return false;
                    }

                    _context.Clients.Remove(result);
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

        public async Task<IEnumerable<Core.Models.Client>> GetAllAsync()
        {
            return await _context.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.ClientSecrets)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.AllowedScopes)
                .Include(c => c.IdentityProviderRestrictions)
                .Include(c => c.Claims)
                .Include(c => c.AllowedCorsOrigins)
                .Select(c => c.ToDomain())
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<Core.Models.Client> GetClientByIdAsync(string clientId)
        {
            var client = await _context.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.ClientSecrets)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.AllowedScopes)
                .Include(c => c.IdentityProviderRestrictions)
                .Include(c => c.Claims)
                .Include(c => c.AllowedCorsOrigins)
                .FirstOrDefaultAsync(c => c.ClientId == clientId).ConfigureAwait(false);
            if (client == null)
            {
                return null;
            }

            return client.ToDomain();
        }

        public async Task<bool> InsertAsync(Core.Models.Client client)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var record = client.ToEntity();
                    _context.Clients.Add(record);
                    await _context.SaveChangesAsync().ConfigureAwait(false); ;
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

        public async Task<bool> UpdateAsync(Core.Models.Client client)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var record = client.ToEntity();
                    var result = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == record.ClientId).ConfigureAwait(false);
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
                    await _context.SaveChangesAsync().ConfigureAwait(false); ;
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

        public async Task<bool> RemoveAllAsync()
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    _context.Clients.RemoveRange(_context.Clients);
                    await _context.SaveChangesAsync().ConfigureAwait(false); ;
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
