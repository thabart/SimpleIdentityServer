using SimpleIdServer.Storage;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Store
{
    internal sealed class DefaultConfirmationCode : IConfirmationCodeStore
    {
        private static string CACHE_KEY = "confirmation_code_{0}";
        private IStorage _storage;

        public DefaultConfirmationCode(IStorage storage)
        {
            _storage = storage;
        }

        public async Task<bool> Add(ConfirmationCode confirmationCode)
        {
            if (confirmationCode == null)
            {
                throw new ArgumentNullException(nameof(confirmationCode));
            }
            
            var key = string.Format(CACHE_KEY, confirmationCode.Value);
            var confirmation = await _storage.TryGetValueAsync<ConfirmationCode>(key);
            if (confirmation != null)
            {
                return false;
            }

            await _storage.SetAsync(key, confirmationCode);
            return true;
        }

        public Task<ConfirmationCode> Get(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            var key = string.Format(CACHE_KEY, code);
            return _storage.TryGetValueAsync<ConfirmationCode>(key);
        }

        public async Task<bool> Remove(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            var key = string.Format(CACHE_KEY, code);
            var confirmation = await _storage.TryGetValueAsync<ConfirmationCode>(key);
            if (confirmation == null)
            {
                return false;
            }

            await _storage.RemoveAsync(code);
            return true;
        }
    }
}
