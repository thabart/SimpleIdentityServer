using System;
using System.Collections.Generic;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt.Encrypt.Encryption;

namespace SimpleIdentityServer.Core.Jwt.Encrypt
{
    public interface IJweGenerator
    {
        string GenerateJwe(
            string entry,
            JweAlg alg,
            JweEnc enc,
            JsonWebKey jsonWebKey);
    }

    public class JweGenerator : IJweGenerator
    {
        private readonly Dictionary<JweEnc, IEncryption> _mappingJweEncToKeySize;

        public JweGenerator(IAesEncryptionHelper aesEncryptionHelper)
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

        public string GenerateJwe(
            string entry,
            JweAlg alg,
            JweEnc enc,
            JsonWebKey jsonWebKey)
        {
            if (jsonWebKey == null)
            {
                return entry;
            }

            // Construct the JWE protected header
            var jweProtectedHeader = new JweProtectedHeader
            {
                Alg = Enum.GetName(typeof(JweAlg), alg),
                Enc = Enum.GetName(typeof(JweEnc), enc),
                Kid = jsonWebKey.Kid
            };

            var algorithm = _mappingJweEncToKeySize[enc];
            var encryptionResult = algorithm.Encrypt(entry, 
                alg, 
                jweProtectedHeader, 
                jsonWebKey);

            var base64EncodedjweProtectedHeaderSerialized = jweProtectedHeader.SerializeWithDataContract().Base64Encode();
            var base64EncodedJweEncryptedKey = Convert.ToBase64String(encryptionResult.EncryptedContentEncryptionKey);
            var base64EncodedIv = Convert.ToBase64String(encryptionResult.Iv);
            var base64EncodedCipherText = Convert.ToBase64String(encryptionResult.CipherText);
            var base64EncodedAuthenticationTag = Convert.ToBase64String(encryptionResult.AuthenticationTag);

            return base64EncodedjweProtectedHeaderSerialized + "." +
                   base64EncodedJweEncryptedKey + "." +
                   base64EncodedIv + "." +
                   base64EncodedCipherText + "." +
                   base64EncodedAuthenticationTag;
        }
    }
}
