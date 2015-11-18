using SimpleIdentityServer.DataAccess.Fake.Models;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SimpleIdentityServer.Api.Configuration
{
    public class JsonWebKeys
    {
        public static List<JsonWebKey> Get()
        {
            var serializedRsa = string.Empty;
            using (var provider = new RSACryptoServiceProvider())
            {
                serializedRsa = provider.ToXmlString(true);
            }

            return new List<JsonWebKey>
            {
                new JsonWebKey
                {
                    Alg = AllAlg.RS256,
                    KeyOps = new []
                    {
                        KeyOperations.Sign
                    },
                    Kid = "1",
                    Kty = KeyType.RSA,
                    SerializedKey = serializedRsa,
                    Use = Use.Sig
                },
                new JsonWebKey
                {
                    Alg = AllAlg.RSA1_5,
                    KeyOps = new []
                    {
                        KeyOperations.Encrypt
                    },
                    Kid = "2",
                    Kty = KeyType.RSA,
                    SerializedKey = serializedRsa,
                    Use = Use.Enc
                }
            };
        }
    }
}