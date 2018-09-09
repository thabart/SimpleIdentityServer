using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Common.Repositories;
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

        #region Constructor

        public RotateJsonWebKeysOperation(IJsonWebKeyRepository jsonWebKeyRepository)
        {
            _jsonWebKeyRepository = jsonWebKeyRepository;
        }

        #endregion

        #region Public methods

        public async Task<bool> Execute()
        {
            var jsonWebKeys = await _jsonWebKeyRepository.GetAllAsync().ConfigureAwait(false);
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
                await _jsonWebKeyRepository.UpdateAsync(jsonWebKey).ConfigureAwait(false);
            }

            return true;
        }

        #endregion
    }
}
