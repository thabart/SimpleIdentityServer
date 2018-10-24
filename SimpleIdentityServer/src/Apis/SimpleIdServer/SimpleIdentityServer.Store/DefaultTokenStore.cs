using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdServer.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Store
{
    internal sealed class DefaultTokenStore : ITokenStore
    {
        private static string GRANTED_TOKENS = "granted_tokens";
        private static string REFRESH_TOKEN = "refresh_token_{0}";
        private static string ACCESS_TOKEN = "access_token_{0}";
        private readonly IStorage _storage;

        public DefaultTokenStore(IStorage storage)
        {
            _storage = storage;
        }

        public async Task<GrantedToken> GetToken(string scopes, string clientId, JwsPayload idTokenJwsPayload, JwsPayload userInfoJwsPayload)
        {
            var tokens = await _storage.TryGetValueAsync<List<GrantedToken>>(GRANTED_TOKENS).ConfigureAwait(false);
            if (tokens == null || !tokens.Any())
            {
                return null;
            }

            var grantedTokens = tokens.Where(g => g.Scope == scopes && g.ClientId == clientId)
                .OrderByDescending(g => g.CreateDateTime);
            if (grantedTokens == null || !grantedTokens.Any())
            {
                return null;
            }
            
            foreach (var grantedToken in grantedTokens)
            {
                if (grantedToken.IdTokenPayLoad != null || idTokenJwsPayload != null)
                {
                    if (grantedToken.IdTokenPayLoad == null || idTokenJwsPayload == null)
                    {
                        continue;
                    }

                    if (!CompareJwsPayload(idTokenJwsPayload, grantedToken.IdTokenPayLoad))
                    {
                        continue;
                    }
                }

                if (grantedToken.UserInfoPayLoad != null || userInfoJwsPayload != null)
                {
                    if (grantedToken.UserInfoPayLoad == null || userInfoJwsPayload == null)
                    {
                        continue;
                    }

                    if (!CompareJwsPayload(userInfoJwsPayload, grantedToken.UserInfoPayLoad))
                    {
                        continue;
                    }
                }

                return grantedToken;
            }

            return null;
        }

        public Task<GrantedToken> GetRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            var key = string.Format(REFRESH_TOKEN, refreshToken);
            return _storage.TryGetValueAsync<GrantedToken>(key);
        }

        public Task<GrantedToken> GetAccessToken(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            var key = string.Format(ACCESS_TOKEN, accessToken);
            return _storage.TryGetValueAsync<GrantedToken>(key);
        }

        public async Task<bool> AddToken(GrantedToken grantedToken)
        {
            if (grantedToken == null)
            {
                throw new ArgumentNullException(nameof(grantedToken));
            }

            var refreshToken = await GetRefreshToken(grantedToken.RefreshToken);
            var accessToken = await GetAccessToken(grantedToken.AccessToken);
            if (refreshToken != null || accessToken != null)
            {
                return false;
            }
            
            var tokens = await _storage.TryGetValueAsync<List<GrantedToken>>(GRANTED_TOKENS).ConfigureAwait(false);
            if (tokens == null)
            {
                tokens = new List<GrantedToken>();
            }

            tokens.Add(grantedToken);
            await _storage.SetAsync(string.Format(ACCESS_TOKEN, grantedToken.AccessToken), grantedToken).ConfigureAwait(false);
            await _storage.SetAsync(string.Format(REFRESH_TOKEN, grantedToken.RefreshToken), grantedToken).ConfigureAwait(false);
            await _storage.SetAsync(GRANTED_TOKENS, tokens).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> RemoveRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            var key = string.Format(REFRESH_TOKEN, refreshToken);
            var rt = await _storage.TryGetValueAsync<GrantedToken>(key).ConfigureAwait(false);
            if (rt == null)
            {
                return false;
            }

            await _storage.RemoveAsync(key).ConfigureAwait(false);
            return false;
        }

        public async Task<bool> RemoveAccessToken(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            var key = string.Format(ACCESS_TOKEN, accessToken);
            var gt = await _storage.TryGetValueAsync<GrantedToken>(key).ConfigureAwait(false);
            if (gt == null)
            {
                return false;
            }

            var grantedTokens = await _storage.TryGetValueAsync<List<GrantedToken>>(GRANTED_TOKENS).ConfigureAwait(false);
            var grantedToken = grantedTokens.FirstOrDefault(g => g.AccessToken == gt.AccessToken);
            grantedTokens.Remove(grantedToken);
            await _storage.RemoveAsync(string.Format(ACCESS_TOKEN, accessToken)).ConfigureAwait(false);
            await _storage.SetAsync(GRANTED_TOKENS, grantedTokens).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> Clean()
        {
            var grantedTokens = await _storage.TryGetValueAsync<List<GrantedToken>>(GRANTED_TOKENS).ConfigureAwait(false);
            foreach(var grantedToken in grantedTokens)
            {
                await RemoveAccessToken(grantedToken.AccessToken).ConfigureAwait(false);
                await RemoveRefreshToken(grantedToken.RefreshToken).ConfigureAwait(false);
            }

            await _storage.RemoveAsync(GRANTED_TOKENS).ConfigureAwait(false);
            return true;
        }

        private static bool CompareJwsPayload(JwsPayload firstJwsPayload, JwsPayload secondJwsPayload)
        {
            foreach (var record in firstJwsPayload)
            {
                if (!Core.Jwt.Constants.AllStandardResourceOwnerClaimNames.Contains(record.Key))
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
    }
}
