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
                var parameters = provider.ExportParameters(false);
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
                    SerializedKey = serializedRsa
                }
            };
        }
    }
}