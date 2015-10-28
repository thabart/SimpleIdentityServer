using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.Repositories
{
    public interface IScopeRepository
    {
        bool InsertScope(Scope scope);

        Scope GetScopeByName(string name);
    }
}
