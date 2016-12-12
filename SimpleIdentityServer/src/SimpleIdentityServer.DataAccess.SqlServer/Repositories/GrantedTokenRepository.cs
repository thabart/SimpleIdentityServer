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
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IsolationLevel = System.Data.IsolationLevel;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public class GrantedTokenRepository : IGrantedTokenRepository
    {
        private readonly SimpleIdentityServerContext _context;

        private readonly IManagerEventSource _managerEventSource;

        public GrantedTokenRepository(
            SimpleIdentityServerContext context,
            IManagerEventSource managerEventSource) {
            _context = context;
            _managerEventSource = managerEventSource;
        }

        public async Task<GrantedToken> GetTokenAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            var grantedToken = await _context.GrantedTokens.FirstOrDefaultAsync(g => g.AccessToken == accessToken).ConfigureAwait(false);
            if (grantedToken == null)
            {
                return null;
            }

            return grantedToken.ToDomain();
        }

        public async Task<GrantedToken> GetTokenByRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            var grantedToken = await _context.GrantedTokens.FirstOrDefaultAsync(g => g.RefreshToken == refreshToken).ConfigureAwait(false);
            if (grantedToken == null)
            {
                return null;
            }

            return grantedToken.ToDomain();
        }

        public async Task<GrantedToken> GetTokenAsync(string scopes, string clientId, JwsPayload idTokenJwsPayload, JwsPayload userInfoJwsPayload)
        {
            var grantedTokens = await _context.GrantedTokens
                .Where(g => g.Scope == scopes && g.ClientId == clientId)
                .OrderByDescending(g => g.CreateDateTime)
                .ToListAsync()
                .ConfigureAwait(false);
            if (grantedTokens == null || !grantedTokens.Any())
            {
                return null;
            }

            foreach (var grantedToken in grantedTokens)
            {
                if (!string.IsNullOrWhiteSpace(grantedToken.IdTokenPayLoad) ||
                    idTokenJwsPayload != null)
                {
                    if (string.IsNullOrWhiteSpace(grantedToken.IdTokenPayLoad) ||
                        idTokenJwsPayload == null)
                    {
                        continue;
                    }

                    var grantedTokenIdTokenJwsPayload = grantedToken.IdTokenPayLoad.DeserializeWithJavascript<JwsPayload>();
                    if (!CompareJwsPayload(idTokenJwsPayload, grantedTokenIdTokenJwsPayload))
                    {
                        continue;
                    }
                }

                if (!string.IsNullOrWhiteSpace(grantedToken.UserInfoPayLoad) ||
                    userInfoJwsPayload != null)
                {
                    if (string.IsNullOrWhiteSpace(grantedToken.UserInfoPayLoad) ||
                        userInfoJwsPayload == null)
                    {
                        continue;
                    }

                    var grantedTokenUserInfoJwsPayload = grantedToken.UserInfoPayLoad.DeserializeWithJavascript<JwsPayload>();
                    if (!CompareJwsPayload(userInfoJwsPayload, grantedTokenUserInfoJwsPayload))
                    {
                        continue;
                    }
                }

                return grantedToken.ToDomain();
            }

            return null;
        }
        
        public async Task<bool> InsertAsync(GrantedToken grantedToken)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var record = new Models.GrantedToken
                    {
                        Id = Guid.NewGuid().ToString(),
                        AccessToken = grantedToken.AccessToken,
                        ClientId = grantedToken.ClientId,
                        CreateDateTime = grantedToken.CreateDateTime,
                        ExpiresIn = grantedToken.ExpiresIn,
                        RefreshToken = grantedToken.RefreshToken,
                        Scope = string.Join(" ", grantedToken.Scope),
                        ParentTokenId = grantedToken.ParentTokenId,
                        IdTokenPayLoad = grantedToken.IdTokenPayLoad == null ? string.Empty : grantedToken.IdTokenPayLoad.SerializeWithJavascript(),
                        UserInfoPayLoad = grantedToken.UserInfoPayLoad == null ? string.Empty : grantedToken.UserInfoPayLoad.SerializeWithJavascript()
                    };

                    _context.GrantedTokens.Add(record);
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

        public async Task<bool> DeleteAsync(GrantedToken grantedToken)
        {
            if (grantedToken == null)
            {
                throw new ArgumentNullException(nameof(grantedToken));
            }

            using (var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead).ConfigureAwait(false))
            {
                try
                {
                    var token = _context.GrantedTokens
                        .Include(g => g.Children)
                        .FirstOrDefault(g => g.AccessToken == grantedToken.AccessToken);
                    RecursiveDelete(token.Children);
                    _context.GrantedTokens.Remove(token);
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

        public async Task<bool> UpdateAsync(GrantedToken grantedToken)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var token = await _context.GrantedTokens.FirstOrDefaultAsync(g => g.AccessToken == grantedToken.AccessToken).ConfigureAwait(false);
                    token.RefreshToken = grantedToken.RefreshToken;
                    _context.GrantedTokens.Update(token);
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

        private static bool CompareJwsPayload(JwsPayload firstJwsPayload, JwsPayload secondJwsPayload)
        {
            foreach (var record in firstJwsPayload)
            {
                if (!Constants.AllStandardResourceOwnerClaimNames.Contains(record.Key))
                {
                    continue;
                }

                if (!secondJwsPayload.ContainsKey(record.Key))
                {
                    return false;
                }

                if (!string.Equals(
                    record.Value.ToString(),
                    secondJwsPayload[record.Key].ToString(),
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private void RecursiveDelete(IList<Models.GrantedToken> children)
        {
            foreach (var child in children)
            {
                var subToken = _context.GrantedTokens.Include(g => g.Children)
                    .FirstOrDefault(g => g.Id == child.Id);
                if (subToken.Children != null && subToken.Children.Any())
                {
                    RecursiveDelete(subToken.Children);
                }

                _context.GrantedTokens.Remove(child);
            }
        }
    }
}
