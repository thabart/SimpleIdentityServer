using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Stores
{
    public interface ITokenStore
    {
        /// <summary>
        /// Try to get a valid access token.
        /// </summary>
        /// <param name="scopes"></param>
        /// <param name="clientId"></param>
        /// <param name="idTokenJwsPayload"></param>
        /// <param name="userInfoJwsPayload"></param>
        /// <returns></returns>
        Task<GrantedToken> GetToken(string scopes, string clientId, JwsPayload idTokenJwsPayload, JwsPayload userInfoJwsPayload);
        Task<GrantedToken> GetRefreshToken(string GetRefreshToken);
        Task<GrantedToken> GetAccessToken(string accessToken);
        Task<bool> AddToken(GrantedToken grantedToken);
        Task<bool> RemoveRefreshToken(string refreshToken);
        Task<bool> RemoveAccessToken(string accessToken);
    }

    internal sealed class InMemoryTokenStore : ITokenStore
    {
        private Dictionary<string, GrantedToken> _tokens;
        private Dictionary<string, string> _mappingStrToRefreshTokens;
        private Dictionary<string, string> _mappingStrToAccessTokens;

        public InMemoryTokenStore()
        {
            _tokens = new Dictionary<string, GrantedToken>();
            _mappingStrToRefreshTokens = new Dictionary<string, string>();
            _mappingStrToAccessTokens = new Dictionary<string, string>();
        }

        public Task<GrantedToken> GetToken(string scopes, string clientId, JwsPayload idTokenJwsPayload, JwsPayload userInfoJwsPayload)
        {
            if (_tokens == null || !_tokens.Any())
            {
                return Task.FromResult((GrantedToken)null);
            }
            
            var grantedTokens = _tokens.Values
                .Where(g => g.Scope == scopes && g.ClientId == clientId)
                .OrderByDescending(g => g.CreateDateTime);
            if (grantedTokens == null || !_tokens.Any())
            {
                return Task.FromResult((GrantedToken)null);
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

                return Task.FromResult(grantedToken);
            }

            return Task.FromResult((GrantedToken)null);
        }

        public Task<GrantedToken> GetRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            if (!_mappingStrToRefreshTokens.ContainsKey(refreshToken))
            {
                return Task.FromResult((GrantedToken)null);
            }

            return Task.FromResult(_tokens[_mappingStrToRefreshTokens[refreshToken]]);
        }

        public Task<GrantedToken> GetAccessToken(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            if (!_mappingStrToAccessTokens.ContainsKey(accessToken))
            {
                return Task.FromResult((GrantedToken)null);
            }

            return Task.FromResult(_tokens[_mappingStrToAccessTokens[accessToken]]);
        }

        public Task<bool> AddToken(GrantedToken grantedToken)
        {
            if (grantedToken == null)
            {
                throw new ArgumentNullException(nameof(grantedToken));
            }

            if (_mappingStrToRefreshTokens.ContainsKey(grantedToken.RefreshToken) 
                || _mappingStrToAccessTokens.ContainsKey(grantedToken.AccessToken))
            {
                return Task.FromResult(false);
            }

            var id = Guid.NewGuid().ToString();
            _tokens.Add(id, grantedToken);
            _mappingStrToRefreshTokens.Add(grantedToken.RefreshToken, id);
            _mappingStrToAccessTokens.Add(grantedToken.AccessToken, id);
            if (!string.IsNullOrWhiteSpace(grantedToken.IdToken))
            {
                _mappingStrToAccessTokens.Add(grantedToken.IdToken, id);
            }

            return Task.FromResult(true);
        }

        public Task<bool> RemoveRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            if (!_mappingStrToRefreshTokens.ContainsKey(refreshToken))
            {
                return Task.FromResult(false);
            }

            _mappingStrToRefreshTokens.Remove(refreshToken);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveAccessToken(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            if (!_mappingStrToAccessTokens.ContainsKey(accessToken))
            {
                return Task.FromResult(false);
            }

            _mappingStrToAccessTokens.Remove(accessToken);
            return Task.FromResult(true);
        }

        private static bool CompareJwsPayload(JwsPayload firstJwsPayload, JwsPayload secondJwsPayload)
        {
            foreach (var record in firstJwsPayload)
            {
                if (!Jwt.Constants.AllStandardResourceOwnerClaimNames.Contains(record.Key))
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
