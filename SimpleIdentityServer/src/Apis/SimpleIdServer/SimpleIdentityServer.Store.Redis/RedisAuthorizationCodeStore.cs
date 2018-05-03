using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Stores;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Store.Redis
{
    internal sealed class RedisAuthorizationCodeStore : IAuthorizationCodeStore
    {
        private readonly RedisStorage _storage;

        public RedisAuthorizationCodeStore(RedisStorage storage)
        {
            _storage = storage;
        }

        public async Task<bool> AddAuthorizationCode(AuthorizationCode authorizationCode)
        {
            if (authorizationCode == null)
            {
                throw new ArgumentNullException(nameof(authorizationCode));
            }

            var authCode = await _storage.TryGetValueAsync<AuthorizationCode>(authorizationCode.Code);
            if (authCode != null)
            {
                return false;
            }

            // TH : Externalize the date.
            await _storage.SetAsync(authorizationCode.Code, authorizationCode, 3600);
            return true;
        }

        public async Task<AuthorizationCode> GetAuthorizationCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            var authCode = await _storage.TryGetValueAsync<AuthorizationCode>(code);
            if (authCode == null)
            {
                return null;
            }

            return authCode;
        }

        public async Task<bool> RemoveAuthorizationCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(code);
            }
            
            var authCode = await _storage.TryGetValueAsync<AuthorizationCode>(code);
            if (authCode == null)
            {
                return false;
            }

            await _storage.RemoveAsync(code);
            return true;
        }
    }
}
