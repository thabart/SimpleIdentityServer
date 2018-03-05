using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Store.Redis
{
    internal sealed class RedisTokenStore : ITokenStore
    {
        private readonly RedisStorage _storage;

        public RedisTokenStore(RedisStorage storage)
        {
            _storage = storage;
        }

        public async Task<bool> AddToken(GrantedToken grantedToken)
        {
            if (grantedToken == null)
            {
                throw new ArgumentNullException(nameof(grantedToken));
            }            

            var id = Guid.NewGuid().ToString();
            await _storage.SetAsync(id, grantedToken, grantedToken.ExpiresIn); // 1. Store tokens.
            await _storage.SetAsync("access_token_" + grantedToken.AccessToken, id, grantedToken.ExpiresIn);
            await _storage.SetAsync("refresh_token_" + grantedToken.RefreshToken, id, grantedToken.ExpiresIn);
            var searchKey = GetSearchKey(grantedToken);
            var grantedTokens = await _storage.GetValue(searchKey);
            var values = new JArray();
            if (!string.IsNullOrWhiteSpace(grantedTokens))
            {
                values = JArray.Parse(grantedTokens);
            }

            values.Add(id);
            await _storage.SetAsync(searchKey, values.ToString(), grantedToken.ExpiresIn); // 2. Store search key.
            return true;
        }

        public async Task<IEnumerable<GrantedToken>> GetTokens(IEnumerable<string> tokenIds)
        {
            if (tokenIds == null)
            {
                throw new ArgumentNullException(nameof(tokenIds));
            }

            var tasks = new List<Task<GrantedToken>>();
            foreach(var tokenId in tokenIds)
            {
                tasks.Add(_storage.TryGetValueAsync<GrantedToken>(tokenId));
            }

            var result = await Task.WhenAll(tasks);
            return result.Where(r => r != null);
        }

        public async Task<GrantedToken> GetAccessToken(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            var tokenId = await _storage.GetValue("access_token_" + accessToken);
            if (string.IsNullOrWhiteSpace(tokenId))
            {
                return null;
            }

            return await _storage.TryGetValueAsync<GrantedToken>(tokenId);
        }

        public async Task<GrantedToken> GetRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            var tokenId = await _storage.GetValue("refresh_token_" + refreshToken);
            if (string.IsNullOrWhiteSpace(tokenId))
            {
                return null;
            }

            return await _storage.TryGetValueAsync<GrantedToken>(tokenId);
        }

        public async Task<GrantedToken> GetToken(string scopes, string clientId, JwsPayload idTokenJwsPayload, JwsPayload userInfoJwsPayload)
        {
            var tokenIds = await _storage.GetValue(scopes + "_" + clientId);
            if (string.IsNullOrWhiteSpace(tokenIds))
            {
                return null;
            }

            var tokenIdArr = JArray.Parse(tokenIds);
            var lstTokenId = new List<string>();
            foreach(var record in tokenIdArr)
            {
                lstTokenId.Add(record.ToString());
            }

            var grantedTokens = await GetTokens(lstTokenId);
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

        public async Task<bool> RemoveAccessToken(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            var tokenId = await _storage.GetValue("access_token_" + accessToken);
            if (string.IsNullOrWhiteSpace(tokenId))
            {
                return false;
            }

            await _storage.RemoveAsync("access_token_" + accessToken);
            return true;
        }

        public async Task<bool> RemoveRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }
            
            var tokenId = await _storage.GetValue("refresh_token_" + refreshToken);
            if (string.IsNullOrWhiteSpace(tokenId))
            {
                return false;
            }

            await _storage.RemoveAsync("refresh_token_" + refreshToken);
            return true;
        }

        private static string GetSearchKey(GrantedToken token)
        {
            return token.Scope + "_" + token.ClientId;
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
