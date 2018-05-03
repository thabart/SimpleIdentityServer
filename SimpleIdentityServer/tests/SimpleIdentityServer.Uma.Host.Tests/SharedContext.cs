using SimpleIdentityServer.Core.Common;
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
            ModelSignatureKey = new SimpleIdentityServer.EF.Models.JsonWebKey
            {
                Alg = SimpleIdentityServer.EF.Models.AllAlg.RS256,
                KeyOps = "0,1",
                Kid = "11",
                Kty = SimpleIdentityServer.EF.Models.KeyType.RSA,
                Use = SimpleIdentityServer.EF.Models.Use.Sig,
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
            ModelEncryptionKey = new SimpleIdentityServer.EF.Models.JsonWebKey
            {
                Alg = SimpleIdentityServer.EF.Models.AllAlg.RSA1_5,
                KeyOps = "2,3",
                Kid = "10",
                Kty = SimpleIdentityServer.EF.Models.KeyType.RSA,
                Use = SimpleIdentityServer.EF.Models.Use.Enc,
                SerializedKey = serializedRsa,
            };
            HttpClientFactory = FakeHttpClientFactory.Instance;
        }

        public JsonWebKey EncryptionKey { get; }
        public SimpleIdentityServer.EF.Models.JsonWebKey ModelEncryptionKey { get; }
        public JsonWebKey SignatureKey { get; }
        public SimpleIdentityServer.EF.Models.JsonWebKey ModelSignatureKey { get; }
        public FakeHttpClientFactory HttpClientFactory { get; }
    }
}
