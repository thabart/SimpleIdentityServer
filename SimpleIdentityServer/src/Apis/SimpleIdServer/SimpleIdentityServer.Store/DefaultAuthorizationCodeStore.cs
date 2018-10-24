using SimpleIdentityServer.Core.Common.Models;
using SimpleIdServer.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Store
{
    internal sealed class DefaultAuthorizationCodeStore : IAuthorizationCodeStore
    {
        private static string CACHE_KEY = "authorization_code_{0}";
        private static Dictionary<string, AuthorizationCode> _mappingStringToAuthCodes;
        private IStorage _storage;

        public DefaultAuthorizationCodeStore(IStorage storage)
        {
            _storage = storage;
            _mappingStringToAuthCodes = new Dictionary<string, AuthorizationCode>();
        }

        public Task<AuthorizationCode> GetAuthorizationCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            return _storage.TryGetValueAsync<AuthorizationCode>(string.Format(CACHE_KEY, code));
        }

        public async Task<bool> AddAuthorizationCode(AuthorizationCode authorizationCode)
        {
            if (authorizationCode == null)
            {
                throw new ArgumentNullException(nameof(authorizationCode));
            }

            var key = string.Format(CACHE_KEY, authorizationCode.Code);
            var authCode = await _storage.TryGetValueAsync<AuthorizationCode>(key);
            if (authCode != null)
            {
                return false;
            }

            await _storage.SetAsync(key, authorizationCode);
            return true;
        }

        public async Task<bool> RemoveAuthorizationCode(string authorizationCode)
        {
            if (authorizationCode == null)
            {
                throw new ArgumentNullException(nameof(authorizationCode));
            }

            var key = string.Format(CACHE_KEY, authorizationCode);
            var authCode = await _storage.TryGetValueAsync<AuthorizationCode>(key);
            if (authCode == null)
            {
                return false;
            }

            await _storage.RemoveAsync(key);
            return true;
        }
    }
}
