using SimpleIdentityServer.Core.Repositories;
using System.Linq;
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
            var jsonWebKeys = await _jsonWebKeyRepository.GetAllAsync();
            if (jsonWebKeys == null ||
                !jsonWebKeys.Any())
            {
                return false;
            }

            foreach(var jsonWebKey in jsonWebKeys)
            {
                var serializedRsa = string.Empty;
#if NET46
                using (var provider = new RSACryptoServiceProvider())
                {
                    serializedRsa = provider.ToXmlString(true);
                }
#else
                using (var rsa = new RSAOpenSsl())
                {
                    serializedRsa = rsa.ToXmlString(true);
                }
#endif

                jsonWebKey.SerializedKey = serializedRsa;
                await _jsonWebKeyRepository.UpdateAsync(jsonWebKey);
            }

            return true;
        }

#endregion
    }
}
