using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.Fake;
using SimpleIdentityServer.DataAccess.Fake.Repositories;

namespace SimpleIdentityServer.Core.UnitTests.Fake
{
    public static class FakeFactories
    {
        public static FakeDataSource FakeDataSource = new FakeDataSource();

        public static IClientRepository GetClientRepository()
        {
            return new FakeClientRepository(FakeDataSource);
        }

        public static IScopeRepository GetScopeRepository()
        {
            return new FakeScopeRepository(FakeDataSource);
        }

        public static IResourceOwnerRepository GetResourceOwnerRepository()
        {
            return new FakeResourceOwnerRepository(FakeDataSource);
        }

        public static IConsentRepository GetConsentRepository()
        {
            return new FakeConsentRepository(FakeDataSource);
        }

        public static IJsonWebKeyRepository GetJsonWebKeyRepository()
        {
            return new FakeJsonWebKeyRepository(FakeDataSource);
        }
    }
}
