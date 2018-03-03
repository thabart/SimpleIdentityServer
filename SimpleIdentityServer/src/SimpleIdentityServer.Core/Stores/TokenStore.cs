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
                return null;
            }

            foreach (var kvp in _tokens)
            {
                var grantedToken = kvp.Value;
                var grantedTokenIdTokenJwsPayload = grantedToken.IdTokenPayLoad;
                if (grantedTokenIdTokenJwsPayload == null)
                {
                    continue;
                }

                if (!CompareJwsPayload(idTokenJwsPayload, grantedTokenIdTokenJwsPayload))
                {
                    continue;
                }

                var grantedTokenUserInfoJwsPayload = grantedToken.UserInfoPayLoad;
                if (grantedTokenUserInfoJwsPayload == null)
                {
                    continue;
                }

                if (!CompareJwsPayload(userInfoJwsPayload, grantedTokenUserInfoJwsPayload))
                {
                    continue;
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
                return null;
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
                return null;
            }

            return Task.FromResult(_tokens[_mappingStrToAccessTokens[accessToken]]);
        }

        public Task<bool> AddToken(GrantedToken grantedToken)
        {
            if (grantedToken == null)
            {
                throw new ArgumentNullException(nameof(grantedToken));
            }

            if (!_mappingStrToRefreshTokens.ContainsKey(grantedToken.RefreshToken) 
                || !_mappingStrToAccessTokens.ContainsKey(grantedToken.AccessToken))
            {
                return Task.FromResult(false);
            }

            var id = Guid.NewGuid().ToString();
            _tokens.Add(id, grantedToken);
            _mappingStrToRefreshTokens.Add(grantedToken.RefreshToken, id);
            _mappingStrToAccessTokens.Add(grantedToken.AccessToken, id);
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

            var grantedToken = _tokens[_mappingStrToAccessTokens[accessToken]];
            _mappingStrToAccessTokens.Remove(accessToken);
            _mappingStrToRefreshTokens.Remove(grantedToken.RefreshToken);
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
