using System.Linq;
using SimpleIdentityServer.Core.Repositories;

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


        public Core.Models.Scope GetScopeByName(string name)
        {
            var scope = FakeDataSource.Instance().Scopes.SingleOrDefault(s => s.Name == name);
            return new Core.Models.Scope
            {
                Name = scope.Name,
                Description = scope.Description,
                IsInternal = scope.IsInternal
            };
        }
    }
}
