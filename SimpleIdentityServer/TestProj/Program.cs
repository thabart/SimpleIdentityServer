using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Extensions;

namespace TestProj
{
    class Program
    {
        private static void Sign()
        {
            var toEncrypt = "coucou";
            byte[] b,
                bytes;
            string exported;
            string exportedPublicKey;
            using (var rsaCryptoServiceProvider = new RSACryptoServiceProvider())
            {
                var parameters = rsaCryptoServiceProvider.ExportParameters(false);
                exported = rsaCryptoServiceProvider.ToXmlString(true);
                exportedPublicKey = rsaCryptoServiceProvider.ToXmlString(false);
            }

            using (var provider = new RSACryptoServiceProvider())
            {
                bytes = ASCIIEncoding.ASCII.GetBytes(toEncrypt);
                provider.FromXmlString(exported);
                b = provider.SignData(bytes, "SHA256");
            }

            // We need "MODULE" + "Exponent" to check the signature.
            using (var provider2 = new RSACryptoServiceProvider())
            {
                provider2.FromXmlString(exportedPublicKey);
                var result = provider2.VerifyData(bytes, "SHA256", b);
                Console.WriteLine(result);
            }

            using (var ec = new ECDiffieHellmanCng())
            {
                ec.ToXmlString(ECKeyXmlFormat.Rfc4050);
            }
        }

        private static string Decrypt(string parameter, string parameters)
        {
            var dataToEncrypt = Convert.FromBase64String(parameter);
            var encryptedBytes = RsaDecrypt(dataToEncrypt, parameters, false);
            return ASCIIEncoding.ASCII.GetString(encryptedBytes);
        }

        private static byte[] RsaDecrypt(byte[] dataToEncrypt, string rsaKeyInfo, bool DoOAEPPadding)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(rsaKeyInfo);
                return rsa.Decrypt(dataToEncrypt, DoOAEPPadding);
            }
        }

        private static byte[] GenerateRandomBytes(int size)
        {
            var data = new byte[size/8];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(data);
                return data;
            }
        }

        private static byte[][] SplitByteArrayInHalf(byte[] arr)
        {
            var halfIndex = arr.Length/2;
            var firstHalf = new byte[halfIndex];
            var secondHalf = new byte[halfIndex];
            Buffer.BlockCopy(arr, 0, firstHalf, 0, halfIndex);
            Buffer.BlockCopy(arr, halfIndex, secondHalf, 0, halfIndex);
            return new[]
            {
                firstHalf,
                secondHalf
            };
        }

        public static byte[] Concat(params byte[][] arrays)
        {
            byte[] result = new byte[arrays.Sum(a => (a == null) ? 0 : a.Length)];
            int offset = 0;

            foreach (byte[] array in arrays)
            {
                if (array == null) continue;

                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        public static byte[] LongToBytes(long value)
        {
            ulong _value = (ulong)value;

            return BitConverter.IsLittleEndian
                ? new[] { (byte)((_value >> 56) & 0xFF), (byte)((_value >> 48) & 0xFF), (byte)((_value >> 40) & 0xFF), (byte)((_value >> 32) & 0xFF), (byte)((_value >> 24) & 0xFF), (byte)((_value >> 16) & 0xFF), (byte)((_value >> 8) & 0xFF), (byte)(_value & 0xFF) }
                : new[] { (byte)(_value & 0xFF), (byte)((_value >> 8) & 0xFF), (byte)((_value >> 16) & 0xFF), (byte)((_value >> 24) & 0xFF), (byte)((_value >> 32) & 0xFF), (byte)((_value >> 40) & 0xFF), (byte)((_value >> 48) & 0xFF), (byte)((_value >> 56) & 0xFF) };
        }

        public static bool ConstantTimeEquals(byte[] expected, byte[] actual)
        {
            if (expected == actual)
                return true;

            if (expected == null || actual == null)
                return false;

            if (expected.Length != actual.Length)
                return false;

            bool equals = true;

            for (int i = 0; i < expected.Length; i++)
                if (expected[i] != actual[i])
                    equals = false;

            return equals;
        }

        #region Encryption

        private static string GenerateJwe(
            string toEncrypt,
            out string rsaXml,
            Func<byte[][], byte[]> hmacKeyParser = null)
        {
            var cekSize = 256;
            string parameters;
            
            // Generate a random Content encryption key with 256 bit.
            var contentEncryptionKey = GenerateRandomBytes(cekSize);

            // Encrypt the Content Encryption Key with RSA algorithm : RSA1_5
            // Encrypt the "Content encryption key" with the public key of the client.
            byte[] encryptedEncryptionKey;
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsaXml = rsa.ToXmlString(true);
                encryptedEncryptionKey = rsa.Encrypt(contentEncryptionKey, false);
            }
            
            // Fetch the MAC_KEY
            // In some case the HASH_MAC KEY is the client_secret : shared key
            var contentEncryptionKeySplitted = SplitByteArrayInHalf(contentEncryptionKey);
            byte[] hmacKey;
            if (hmacKeyParser == null)
            {
                hmacKey = contentEncryptionKeySplitted[0];
            }
            else
            {
                hmacKey = hmacKeyParser(contentEncryptionKeySplitted);
            }

            var aesCbcKey = contentEncryptionKeySplitted[1];

            // Initialize the vector value.
            // IV size equal : key size / 2
            var iv = GenerateRandomBytes(128);

            byte[] ciptherText;

            // Encrypt the plain text to create ciphertext.
            using (var aes = new AesManaged())
            {
                aes.Key = aesCbcKey;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (var swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(toEncrypt);
                            }

                            ciptherText = msEncrypt.ToArray();
                        }
                    }
                }
            }

            // Concatenate the AAD + IV + ciphertext + AL

            var protectedHeader = new Dictionary<string, string>
            {
                {
                    "alg", "RSA1_5"
                },
                {
                    "enc", "A128CBC_HS256"
                },
                {
                    "kid", "1"
                }
            };

            // base64 encoded protected header
            var jweProtectedHeaderSerialized = protectedHeader.SerializeWithJavascript();

            var base64EncodedJweProtectedHeader = jweProtectedHeaderSerialized.Base64Encode();

            // jwe encrypted key
            var base64EncodedJweEncryptedKey = Convert.ToBase64String(encryptedEncryptionKey);

            // iv
            var base64EncodedIv = Convert.ToBase64String(iv);

            // Cipther text
            var base64EncodedCipherText = Convert.ToBase64String(ciptherText);

            // Calculate the additional authenticated data
            var aad = Encoding.UTF8.GetBytes(jweProtectedHeaderSerialized);
            var base64EncodedAdd = Convert.ToBase64String(aad);

            // Calculate the authentication tag
            var al = LongToBytes(aad.Length * 8);
            var hmacInput = Concat(aad, iv, ciptherText, al);

            byte[] hmacValue;
            // Sign the HMAC input with HMAC KEY
            using (var crypt = new HMACSHA256(hmacKey))
            {
                hmacValue = crypt.ComputeHash(hmacInput);
            }

            var authenticationTag = SplitByteArrayInHalf(hmacValue)[0];
            var base64AuthenticationTag = Convert.ToBase64String(authenticationTag);

            var result = string.Format("{0}.{1}.{2}.{3}.{4}",
                base64EncodedJweProtectedHeader,
                base64EncodedJweEncryptedKey,
                base64EncodedIv,
                base64EncodedCipherText,
                base64AuthenticationTag);

            Console.WriteLine(result);

            return result;
        }


        private static string GenerateJweWithClientSecretAsHmacKey(
            string toEncrypt,
            string clientSecret,
            out string rsaXml)
        {
            var callback = new Func<byte[][], byte[]>((arg) => Encoding.UTF8.GetBytes(clientSecret));
            return GenerateJwe(toEncrypt, out rsaXml, callback);
        }

        #endregion

        #region Decryption

        private static string DecodeJwe(
            string jwe,
            string rsaXml,
            Func<byte[]> getHmacKeyCallback = null)
        {
            var jweSplitted = jwe.Split('.');

            var jweEncryptedKeyBytes = Convert.FromBase64String(jweSplitted[1]);
            var ivBytes = Convert.FromBase64String(jweSplitted[2]);
            var cipherTextBytes = Convert.FromBase64String(jweSplitted[3]);
            var authenticationTagBytes = Convert.FromBase64String(jweSplitted[4]);

            var jweProtectedHeaderSerialized = jweSplitted[0].Base64Decode();
            var jweEncryptedKey = jweSplitted[1].Base64Decode();
            var iv = jweSplitted[2].Base64Decode();
            var cipherText = jweSplitted[3].Base64Decode();
            var authenticationTag = jweSplitted[4].Base64Decode();
            
            // Decrypt the encryption key with the private key of the client.
            byte[] contentEncryptionKey;
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(rsaXml);
                contentEncryptionKey = rsa.Decrypt(jweEncryptedKeyBytes, false);
            }

            // Calculate the AAD
            var aad = Encoding.UTF8.GetBytes(jweProtectedHeaderSerialized);
            
            var contentEncryptionKeySplitted = SplitByteArrayInHalf(contentEncryptionKey);
            byte[] hmacKey;
            if (getHmacKeyCallback == null)
            {
                hmacKey = contentEncryptionKeySplitted[0];
            }
            else
            {
                hmacKey = getHmacKeyCallback();
            }

            var aesCbcKey = contentEncryptionKeySplitted[1];
            
            // Calculate the authentication tag
            var al = LongToBytes(aad.Length * 8);
            var hmacInput = Concat(aad, ivBytes, cipherTextBytes, al);

            byte[] hmacValue;
            // Sign the HMAC input with HMAC KEY
            using (var crypt = new HMACSHA256(hmacKey))
            {
                hmacValue = crypt.ComputeHash(hmacInput);
            }

            var authenticationTagCalculated = SplitByteArrayInHalf(hmacValue)[0];
            var macAreEquals = ConstantTimeEquals(authenticationTagCalculated, authenticationTagBytes);

            string result;
            // Encrypt the plain text to create ciphertext.
            using (var aes = new AesManaged())
            {
                aes.Key = aesCbcKey;
                aes.IV = ivBytes;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    using (var msDecrypt = new MemoryStream(cipherTextBytes))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }

            return result;
        }
        
        private static string DecodeJweWithClientSecret(
            string jwe,
            string rsaXml,
            string clientSecret)
        {
            var callback = new Func<byte[]>(() => Encoding.UTF8.GetBytes(clientSecret));
            return DecodeJwe(jwe, rsaXml, callback);
        }

        #endregion

        static void Main(string[] args)
        {
            var clientSecret = "ClientSecret";
            string rsaXml;
            var jwe = GenerateJweWithClientSecretAsHmacKey("hello", clientSecret, out rsaXml);

            var decrypted = DecodeJweWithClientSecret(jwe, rsaXml, clientSecret);
            Console.WriteLine(decrypted);
            Console.ReadLine();
            /*
            string parameters;
            var encryptedData = Encrypt("encrypt data", out parameters);
            Console.WriteLine(encryptedData);

            var decryptedData = Decrypt(encryptedData, parameters);
            Console.WriteLine(decryptedData);

            Console.ReadLine();
             */
            /*
            var today = DateTime.UtcNow.AddDays(2).ToString();
            var encoded =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJub25lIiwia2lkIjpudWxsfQ==.eyJpc3MiOiJodHRwOi8vbG9jYWxob3N0L2lkZW50aXR5IiwiYXVkIjpbIk15QmxvZyJdLCJleHAiOjE0NTA2ODYzMDcsImlhdCI6MTQ0NzY4NjMwNywic3ViIjoiYWRtaW5pc3RyYXRvckBob3RtYWlsLmJlIiwiYWNyIjoib3BlbmlkLnBhcGUuYXV0aF9sZXZlbC5ucy5wYXNzd29yZD0xIiwiYW1yIjoicGFzc3dvcmQiLCJhenAiOiJNeUJsb2cifQ==.";
            var arr = encoded.Split('.');
            var header = arr[0].Base64Decode();
            var payload = arr[1].Base64Decode();
            Console.WriteLine(header);
            Console.WriteLine(payload);
            Console.ReadLine();*/
        }
    }
}
