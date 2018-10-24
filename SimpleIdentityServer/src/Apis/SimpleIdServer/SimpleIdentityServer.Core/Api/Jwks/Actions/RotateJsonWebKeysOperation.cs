using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Store;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Jwks.Actions
{
    public interface IRotateJsonWebKeysOperation
    {
        Task<bool> Execute();
    }

    public class RotateJsonWebKeysOperation : IRotateJsonWebKeysOperation
    {
        private readonly IJsonWebKeyRepository _jsonWebKeyRepository;
        private readonly ITokenStore _tokenStore;

        #region Constructor

        public RotateJsonWebKeysOperation(IJsonWebKeyRepository jsonWebKeyRepository, ITokenStore tokenStore)
        {
            _jsonWebKeyRepository = jsonWebKeyRepository;
            _tokenStore = tokenStore;
        }

        #endregion

        #region Public methods

        public async Task<bool> Execute()
        {
            var jsonWebKeys = await _jsonWebKeyRepository.GetAllAsync();
            if (jsonWebKeys == null ||
                !jsonWebKeys.Any())
            {
                return false;
            }

            foreach(var jsonWebKey in jsonWebKeys)
            {
                var serializedRsa = string.Empty;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using (var provider = new RSACryptoServiceProvider())
                    {
                        serializedRsa = provider.ToXmlStringNetCore(true);
                    }
                }
                else
                {
                    using (var rsa = new RSAOpenSsl())
                    {
                        serializedRsa = rsa.ToXmlStringNetCore(true);
                    }
                }

                jsonWebKey.SerializedKey = serializedRsa;
                await _jsonWebKeyRepository.UpdateAsync(jsonWebKey);
            }

            await _tokenStore.Clean().ConfigureAwait(false);
            return true;
        }

        #endregion
    }
}
