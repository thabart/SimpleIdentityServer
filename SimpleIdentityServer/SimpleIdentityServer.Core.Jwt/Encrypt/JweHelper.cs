using SimpleIdentityServer.Core.Jwt.Encrypt.Encryption;
using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Jwt.Encrypt
{
    public interface IJweHelper
    {
        IEncryption GetEncryptor(JweEnc enc);
    }

    public class JweHelper : IJweHelper
    {
        private readonly Dictionary<JweEnc, IEncryption> _mappingJweEncToKeySize;

        public JweHelper(IAesEncryptionHelper aesEncryptionHelper)
        {
            _mappingJweEncToKeySize = new Dictionary<JweEnc, IEncryption>
            {
                {
                    JweEnc.A128CBC_HS256, new AesEncryptionWithHmac(aesEncryptionHelper, 256)
                },
                {
                    JweEnc.A192CBC_HS384, new AesEncryptionWithHmac(aesEncryptionHelper, 384)
                },
                {
                    JweEnc.A256CBC_HS512, new AesEncryptionWithHmac(aesEncryptionHelper, 512)
                }
            };
        }

        public IEncryption GetEncryptor(JweEnc enc)
        {
            return _mappingJweEncToKeySize[enc];
        }
    }
}
