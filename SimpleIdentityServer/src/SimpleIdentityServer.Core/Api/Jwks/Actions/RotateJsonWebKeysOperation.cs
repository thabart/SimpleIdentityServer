using SimpleIdentityServer.Core.Repositories;
using System.Linq;
using System.Security.Cryptography;

namespace SimpleIdentityServer.Core.Api.Jwks.Actions
{
    public interface IRotateJsonWebKeysOperation
    {
        bool Execute();
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

        public bool Execute()
        {
            var jsonWebKeys = _jsonWebKeyRepository.GetAll();
            if (jsonWebKeys == null ||
                !jsonWebKeys.Any())
            {
                return false;
            }

            foreach(var jsonWebKey in jsonWebKeys)
            {
                var serializedRsa = string.Empty;
                using (var provider = new RSACryptoServiceProvider())
                {
                    serializedRsa = provider.ToXmlString(true);
                }

                jsonWebKey.SerializedKey = serializedRsa;
                _jsonWebKeyRepository.Update(jsonWebKey);
            }

            return true;
        }

#endregion
    }
}
