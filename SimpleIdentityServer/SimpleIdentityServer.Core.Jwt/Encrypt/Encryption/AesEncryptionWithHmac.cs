using System.Security.Cryptography;
using System.Text;
using SimpleIdentityServer.Core.Common.Extensions;
using System;

namespace SimpleIdentityServer.Core.Jwt.Encrypt.Encryption
{
    public class AesEncryptionWithHmac : IEncryption
    {
        private readonly IAesEncryptionHelper _aesEncryptionHelper;

        private readonly int _keySize;

        public AesEncryptionWithHmac(
            IAesEncryptionHelper aesEncryptionHelper,
            int keySize)
        {
            _aesEncryptionHelper = aesEncryptionHelper;
            _keySize = keySize;
        }

        public AesEncryptionResult Encrypt(
            string toEncrypt,
            JweAlg alg,
            JweProtectedHeader protectedHeader,
            JsonWebKey jsonWebKey)
        {
            // Get the content encryption key
            var contentEncryptionKey = _aesEncryptionHelper.GenerateContentEncryptionKey(_keySize);

            // Encrypt the content encryption key
            var encryptedContentEncryptionKey = _aesEncryptionHelper.EncryptContentEncryptionKey(
                contentEncryptionKey, 
                alg, 
                jsonWebKey);

            var contentEncryptionKeySplitted = GetKeysFromContentEncryptionKey(contentEncryptionKey);

            var hmacKey = contentEncryptionKeySplitted[0];
            var aesCbcKey = contentEncryptionKeySplitted[1];

            var iv = ByteManipulator.GenerateRandomBytes(_keySize/2);

            // Encrypt the plain text & create cipher text.
            var cipherText = _aesEncryptionHelper.EncryptWithAesAlgorithm(
                toEncrypt,
                aesCbcKey,
                iv);
            
            // Calculate the additional authenticated data.
            var serializedProtectedHeader = protectedHeader.SerializeWithDataContract();
            var aad = Encoding.UTF8.GetBytes(serializedProtectedHeader);

            // Calculate the authentication tag.
            var al = ByteManipulator.LongToBytes(aad.Length * 8);
            var hmacInput = ByteManipulator.Concat(aad, iv, cipherText, al);
            var hmacValue = ComputeHmac(_keySize, hmacKey, hmacInput);
            var authenticationTag = ByteManipulator.SplitByteArrayInHalf(hmacValue)[0];

            return new AesEncryptionResult
            {
                Iv = iv,
                CipherText = cipherText,
                EncryptedContentEncryptionKey = encryptedContentEncryptionKey,
                AuthenticationTag = authenticationTag
            };
        }

        public string Decrypt(
            string toDecrypt,
            JweAlg alg,
            JsonWebKey jsonWebKey)
        {
            try
            {
                var toDecryptSplitted = toDecrypt.Split('.');
                var serializedProtectedHeader = toDecryptSplitted[0].Base64Decode();
                var encryptedContentEncryptionKeyBytes = toDecryptSplitted[1].Base64DecodeBytes();
                var ivBytes = toDecryptSplitted[2].Base64DecodeBytes();
                var cipherText = toDecryptSplitted[3].Base64DecodeBytes();
                var authenticationTag = toDecryptSplitted[4].Base64DecodeBytes();

                var contentEncryptionKey = _aesEncryptionHelper.DecryptContentEncryptionKey(
                    encryptedContentEncryptionKeyBytes,
                    alg,
                    jsonWebKey);
                var contentEncryptionKeySplitted = GetKeysFromContentEncryptionKey(contentEncryptionKey);

                var hmacKey = contentEncryptionKeySplitted[0];
                var aesCbcKey = contentEncryptionKeySplitted[1];

                // Encrypt the plain text & create cipher text.
                var decrypted = _aesEncryptionHelper.DecryptWithAesAlgorithm(
                    cipherText,
                    aesCbcKey,
                    ivBytes);

                // Calculate the additional authenticated data.
                var aad = Encoding.UTF8.GetBytes(serializedProtectedHeader);

                // Calculate the authentication tag.
                var al = ByteManipulator.LongToBytes(aad.Length*8);
                var hmacInput = ByteManipulator.Concat(aad, ivBytes, cipherText, al);
                var hmacValue = ComputeHmac(_keySize, hmacKey, hmacInput);
                var newAuthenticationTag = ByteManipulator.SplitByteArrayInHalf(hmacValue)[0];

                // Check if the authentication tags are equal other raise an exception.
                if (!ByteManipulator.ConstantTimeEquals(newAuthenticationTag, authenticationTag))
                {
                    // TODO : raise an exception.
                    return string.Empty;
                }

                return decrypted;
            }
            catch (Exception ex)
            {
                throw new Exception("invalid " + toDecrypt);
            }
        }

        private byte[] ComputeHmac(
            int keySize,
            byte[] key,
            byte[] input)
        {
            HMAC hashMacAlg;
            switch (keySize)
            {
                case 256:
                    hashMacAlg = new HMACSHA256(key);
                    break;
                case 384:
                    hashMacAlg = new HMACSHA384(key);
                    break;
                case 512:
                    hashMacAlg = new HMACSHA512(key);
                    break;
                default:
                    return null;
            }

            return hashMacAlg.ComputeHash(input);
        }

        private static byte[][] GetKeysFromContentEncryptionKey(byte[] contentEncryptionKey)
        {
            var contentEncryptionKeySplitted = ByteManipulator.SplitByteArrayInHalf(contentEncryptionKey);

            // Fetch the MAC_KEY
            var hmacKey = contentEncryptionKeySplitted[0];

            // Fetch the AES CBC KEY
            var aesCbcKey = contentEncryptionKeySplitted[1];

            return new[]
            {
                hmacKey,
                aesCbcKey
            };
        }
    }
}
