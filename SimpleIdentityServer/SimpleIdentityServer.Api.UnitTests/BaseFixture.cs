using NUnit.Framework;
using SimpleIdentityServer.Api.UnitTests.Fake;
using SimpleIdentityServer.DataAccess.Fake;

namespace SimpleIdentityServer.Api.UnitTests
{
    [TestFixture]
    public class BaseFixture
    {
        [SetUp]
        public void InitializeTheFakeDataSource()
        {
            FakeDataSource.Instance().Clients = FakeOpenIdAssets.GetClients();
            FakeDataSource.Instance().Scopes = FakeOpenIdAssets.GetScopes();
            FakeDataSource.Instance().ResourceOwners = FakeOpenIdAssets.GetResourceOwners();
            FakeDataSource.Instance().Consents = FakeOpenIdAssets.GetConsents();
            FakeDataSource.Instance().JsonWebKeys = FakeOpenIdAssets.GetJsonWebKeys();
        }
    }
}
