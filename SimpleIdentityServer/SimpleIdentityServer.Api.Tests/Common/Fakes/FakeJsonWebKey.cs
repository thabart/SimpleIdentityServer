using SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.Api.Tests.Common.Fakes
{
    public class FakeJsonWebKey
    {
        public string Kid { get; set; }

        public AllAlg Alg { get; set; }

        public KeyOperations Operation { get; set; }

        public KeyType Kty { get; set; }

        public Use Use { get; set; }
    }
}
