using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Uma.Host.Tests.Fakes;
using System.Security.Cryptography;

namespace SimpleIdentityServer.Uma.Host.Tests
{
    public class SharedContext
    {
        public SharedContext()
        {
            var serializedRsa = string.Empty;
            using (var provider = new RSACryptoServiceProvider())
            {
                serializedRsa = provider.ToXmlString(true);
            }

            SignatureKey = new JsonWebKey
            {
                Alg = AllAlg.RS256,
                KeyOps = new KeyOperations[]
                {
                    KeyOperations.Sign,
                    KeyOperations.Verify
                },
                Kid = "11",
                Kty = KeyType.RSA,
                Use = Use.Sig,
                SerializedKey = serializedRsa,
            };
            ModelSignatureKey = new DataAccess.SqlServer.Models.JsonWebKey
            {
                Alg = DataAccess.SqlServer.Models.AllAlg.RS256,
                KeyOps = "0,1",
                Kid = "11",
                Kty = DataAccess.SqlServer.Models.KeyType.RSA,
                Use = DataAccess.SqlServer.Models.Use.Sig,
                SerializedKey = serializedRsa,
            };
            EncryptionKey = new JsonWebKey
            {
                Alg = AllAlg.RSA1_5,
                KeyOps = new[]
                {
                    KeyOperations.Decrypt,
                    KeyOperations.Encrypt
                },
                Kid = "10",
                Kty = KeyType.RSA,
                Use = Use.Enc,
                SerializedKey = serializedRsa,
            };
            ModelEncryptionKey = new DataAccess.SqlServer.Models.JsonWebKey
            {
                Alg = DataAccess.SqlServer.Models.AllAlg.RSA1_5,
                KeyOps = "2,3",
                Kid = "10",
                Kty = DataAccess.SqlServer.Models.KeyType.RSA,
                Use = DataAccess.SqlServer.Models.Use.Enc,
                SerializedKey = serializedRsa,
            };
            HttpClientFactory = FakeHttpClientFactory.Instance;
        }

        public JsonWebKey EncryptionKey { get; }
        public DataAccess.SqlServer.Models.JsonWebKey ModelEncryptionKey { get; }
        public JsonWebKey SignatureKey { get; }
        public DataAccess.SqlServer.Models.JsonWebKey ModelSignatureKey { get; }
        public FakeHttpClientFactory HttpClientFactory { get; }
    }
}
