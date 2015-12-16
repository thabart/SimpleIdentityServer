using System.Linq;

using SimpleIdentityServer.Core.Common.Extensions;

namespace SimpleIdentityServer.Core.Jwt.Encrypt
{
    public interface IJweParser
    {
        string Parse(
            string jwe,
            JsonWebKey jsonWebKey);

        string ParseByUsingSymmetricPassword(
            string jwe,
            JsonWebKey jsonWebKey,
            string password);

        JweProtectedHeader GetHeader(string jwe);
    }

    public class JweParser : IJweParser
    {
        private readonly IJweHelper _jweHelper;

        public JweParser(IJweHelper jweHelper)
        {
            _jweHelper = jweHelper;
        }

        /// <summary>
        /// Try to parse the Json Web Encrypted token.
        /// Returns the Json Web Signed token otherwise null.
        /// </summary>
        /// <param name="jwe"></param>
        /// <param name="jsonWebKey"></param>
        /// <returns></returns>
        public string Parse(
            string jwe,
            JsonWebKey jsonWebKey)
        {
            const string emptyResult = null;
            var header = GetHeader(jwe);
            if (header == null)
            {
                return emptyResult;
            }

            var algorithmName = header.Alg;
            var encryptionName = header.Enc;
            if (!Constants.MappingNameToJweAlgEnum.Keys.Contains(algorithmName)
                || !Constants.MappingNameToJweEncEnum.Keys.Contains(encryptionName))
            {
                return emptyResult;
            }

            var algorithmEnum = Constants.MappingNameToJweAlgEnum[algorithmName];
            var encryptionEnum = Constants.MappingNameToJweEncEnum[encryptionName];

            var algorithm = _jweHelper.GetEncryptor(encryptionEnum);
            return algorithm.Decrypt(jwe, algorithmEnum, jsonWebKey);
        }

        /// <summary>
        /// Try to parse the Json Web Encrypted token with given password
        /// Returns the Json Web Signed token otherwise null.
        /// </summary>
        /// <param name="jwe"></param>
        /// <param name="jsonWebKey"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string ParseByUsingSymmetricPassword(
            string jwe,
            JsonWebKey jsonWebKey,
            string password)
        {
            const string emptyResult = null;
            if (jsonWebKey == null || 
                string.IsNullOrWhiteSpace(jwe))
            {
                return emptyResult;
            }

            var header = GetHeader(jwe);
            if (header == null)
            {
                return emptyResult;
            }

            var algorithmName = header.Alg;
            var encryptionName = header.Enc;
            if (!Constants.MappingNameToJweAlgEnum.Keys.Contains(algorithmName)
                || !Constants.MappingNameToJweEncEnum.Keys.Contains(encryptionName))
            {
                return emptyResult;
            }

            var algorithmEnum = Constants.MappingNameToJweAlgEnum[algorithmName];
            var encryptionEnum = Constants.MappingNameToJweEncEnum[encryptionName];

            var algorithm = _jweHelper.GetEncryptor(encryptionEnum);
            return algorithm.DecryptWithSymmetricPassword(jwe, algorithmEnum, jsonWebKey, password);
        }

        public JweProtectedHeader GetHeader(string jwe)
        {
            var jweSplitted = jwe.Split('.');
            if (!jweSplitted.Any() ||
                jweSplitted.Length < 5)
            {
                return null;
            }

            var protectedHeader = jweSplitted[0].Base64Decode();
            return protectedHeader.DeserializeWithDataContract<JweProtectedHeader>();
        }
    }
}
