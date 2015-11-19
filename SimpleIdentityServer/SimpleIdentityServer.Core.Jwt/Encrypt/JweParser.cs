using System.Linq;

using SimpleIdentityServer.Core.Common.Extensions;

namespace SimpleIdentityServer.Core.Jwt.Encrypt
{
    public interface IJweParser
    {
        string Parse(
            string jwe,
            JsonWebKey jsonWebKey);
    }

    public class JweParser : IJweParser
    {
        private readonly IJweHelper _jweHelper;

        public JweParser(IJweHelper jweHelper)
        {
            _jweHelper = jweHelper;
        }

        public string Parse(
            string jwe,
            JsonWebKey jsonWebKey)
        {
            var header = GetHeader(jwe);
            if (header == null)
            {
                return jwe;
            }

            var algorithmName = header.Alg;
            var encryptionName = header.Enc;
            if (!Constants.MappingNameToJweAlgEnum.Keys.Contains(algorithmName)
                || !Constants.MappingNameToJweEncEnum.Keys.Contains(encryptionName))
            {
                return null;
            }

            var algorithmEnum = Constants.MappingNameToJweAlgEnum[algorithmName];
            var encryptionEnum = Constants.MappingNameToJweEncEnum[encryptionName];

            var algorithm = _jweHelper.GetEncryptor(encryptionEnum);
            return algorithm.Decrypt(jwe, algorithmEnum, jsonWebKey);
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
