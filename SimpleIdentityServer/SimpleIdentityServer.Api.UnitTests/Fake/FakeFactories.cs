using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.Fake.Repositories;

namespace SimpleIdentityServer.Api.UnitTests.Fake
{
    public static class FakeFactories
    {
        public static IClientRepository GetClientRepository()
        {
            return new FakeClientRepository();
        }

        public static IScopeRepository GetScopeRepository()
        {
            return new FakeScopeRepository();
        }

        public static IResourceOwnerRepository GetResourceOwnerRepository()
        {
            return new FakeResourceOwnerRepository();
        }

        public static IConsentRepository GetConsentRepository()
        {
            return new FakeConsentRepository();
        }

        public static IJsonWebKeyRepository GetJsonWebKeyRepository()
        {
            return new FakeJsonWebKeyRepository();
        }
    }
}
