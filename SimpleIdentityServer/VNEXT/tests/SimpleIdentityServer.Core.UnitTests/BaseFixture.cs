using System.Linq;
using SimpleIdentityServer.Core.UnitTests.Fake;
using SimpleIdentityServer.DataAccess.Fake;

namespace SimpleIdentityServer.Core.UnitTests
{
    public class BaseFixture
    {
        public BaseFixture()
        {
            FakeDataSource.Instance().Init();
            FakeDataSource.Instance().Clients = FakeOpenIdAssets.GetClients();
            FakeDataSource.Instance().Scopes = FakeOpenIdAssets.GetScopes();
            FakeDataSource.Instance().ResourceOwners = FakeOpenIdAssets.GetResourceOwners();
            FakeDataSource.Instance().Consents = FakeOpenIdAssets.GetConsents();
            FakeDataSource.Instance().JsonWebKeys = FakeOpenIdAssets.GetJsonWebKeys();
            FakeDataSource.Instance().Clients.First().JsonWebKeys = FakeDataSource.Instance().JsonWebKeys;
        }
    }
}
