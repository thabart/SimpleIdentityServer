using SimpleIdentityServer.Core.Repositories;
using System.Collections.Generic;

using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeScopeRepository : IScopeRepository
    {
        public bool InsertScope(Core.Models.Scope scope)
        {
            FakeDataSource.Instance().Scopes.Add(new MODELS.Scope
            {
                Name = scope.Name
            });

            return true;
        }
    }
}
