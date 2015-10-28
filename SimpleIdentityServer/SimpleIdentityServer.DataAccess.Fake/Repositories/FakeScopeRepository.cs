using System.Linq;
using SimpleIdentityServer.Core.Repositories;

using SimpleIdentityServer.DataAccess.Fake.Extensions;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeScopeRepository : IScopeRepository
    {
        public bool InsertScope(Core.Models.Scope scope)
        {
            FakeDataSource.Instance().Scopes.Add(scope.ToFake());
            return true;
        }


        public Core.Models.Scope GetScopeByName(string name)
        {
            var scope = FakeDataSource.Instance().Scopes.SingleOrDefault(s => s.Name == name);
            return scope.ToBusiness();
        }
    }
}
