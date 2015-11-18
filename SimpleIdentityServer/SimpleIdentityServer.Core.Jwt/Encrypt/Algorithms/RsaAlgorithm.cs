using System;
using System.Security.Cryptography;

namespace SimpleIdentityServer.Core.Jwt.Encrypt.Algorithms
{
    public class RsaAlgorithm : IAlgorithm
    {
        private readonly bool _oaep;

        public RsaAlgorithm(bool oaep)
        {
            _oaep = oaep;
        }

        public byte[] Encrypt(
            byte[] toBeEncrypted,
            JsonWebKey jsonWebKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(jsonWebKey.SerializedKey);
                return rsa.Encrypt(toBeEncrypted, _oaep);
            }
        }
        public byte[] Decrypt(
            byte[] toBeDecrypted, 
            JsonWebKey jsonWebKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(jsonWebKey.SerializedKey);
                return rsa.Decrypt(toBeDecrypted, _oaep);
            }
        }
    }
}
