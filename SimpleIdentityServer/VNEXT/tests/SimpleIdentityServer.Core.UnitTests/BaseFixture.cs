using System.Linq;
using SimpleIdentityServer.Core.UnitTests.Fake;
using SimpleIdentityServer.DataAccess.Fake;

namespace SimpleIdentityServer.Core.UnitTests
{
    public class BaseFixture
    {
        public BaseFixture()
        {
            FakeFactories.FakeDataSource.Init();
            FakeFactories.FakeDataSource.Clients = FakeOpenIdAssets.GetClients();
            FakeFactories.FakeDataSource.Scopes = FakeOpenIdAssets.GetScopes();
            FakeFactories.FakeDataSource.ResourceOwners = FakeOpenIdAssets.GetResourceOwners();
            FakeFactories.FakeDataSource.Consents = FakeOpenIdAssets.GetConsents();
            FakeFactories.FakeDataSource.JsonWebKeys = FakeOpenIdAssets.GetJsonWebKeys();
            FakeFactories.FakeDataSource.Clients.First().JsonWebKeys = FakeFactories.FakeDataSource.JsonWebKeys;
        }
    }
}
