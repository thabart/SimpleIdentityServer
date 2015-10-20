using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories.Scopes
{
    public class ScopeRepository : IScopeRepository
    {
        public bool InsertScope(Scope scope)
        {
            return true;
        }
    }
}
