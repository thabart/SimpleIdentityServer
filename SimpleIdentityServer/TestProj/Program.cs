using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
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
        
        private static byte[] Encrypt(byte[] dataToEncrypt, out string parameters)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                parameters = rsa.ToXmlString(true);
                return RsaEncrypt(dataToEncrypt, rsa.ExportParameters(false), false);
            }
        }

        private static string Decrypt(string parameter, string parameters)
        {
            var dataToEncrypt = Convert.FromBase64String(parameter);
            var encryptedBytes = RsaDecrypt(dataToEncrypt, parameters, false);
            return ASCIIEncoding.ASCII.GetString(encryptedBytes);
        }

        private static byte[] RsaEncrypt(byte[] dataToEncrypt, RSAParameters rsaKeyInfo, bool DoOAEPPadding)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(rsaKeyInfo);
                return rsa.Encrypt(dataToEncrypt, DoOAEPPadding);
            }
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

        private static string GenerateJwe(string toEncrypt)
        {
            var cekSize = 256;
            string parameters;

            // Get the plain text
            var plainText = Encoding.UTF8.GetBytes(toEncrypt);

            // Generate a random Content encryption key with 256 bit.
            var contentEncryptionKey = GenerateRandomBytes(cekSize);

            // Encrypt the Content Encryption Key with RSA algorithm : RSA1_5
            var encryptedEncryptionKey = Encrypt(contentEncryptionKey, out parameters);

            var contentEncryptionKeySplitted = SplitByteArrayInHalf(contentEncryptionKey);

            // Fetch the MAC_KEY
            var hmacKey = contentEncryptionKeySplitted[0];
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
                                swEncrypt.Write(plainText);
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
            var add = Encoding.UTF8.GetBytes(jweProtectedHeaderSerialized);
            var base64EncodedAdd = Convert.ToBase64String(add);

            // Calculate the authentication tag
            var al = LongToBytes(add.Length * 8);
            var hmacInput = Concat(add, iv, ciptherText, al);

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

        private static string DecodeJwe(string jwe)
        {
            var jweSplitted = jwe.Split('.');

            var protectedHeader = jweSplitted[0].Base64Decode();
            var jweEncryptedKey = jweSplitted[1].Base64Decode();
            var vi = jweSplitted[2].Base64Decode();
            var ciptherText = jweSplitted[3].Base64Decode();
            var authenticationTag = jweSplitted[4].Base64Decode();



            Console.WriteLine(protectedHeader);

            return string.Empty;
        }
        
        static void Main(string[] args)
        {
            var jwe = GenerateJwe("hello");
            DecodeJwe(jwe);

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
