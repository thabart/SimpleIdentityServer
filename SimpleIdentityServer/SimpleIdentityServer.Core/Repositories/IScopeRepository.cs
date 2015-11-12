using System.Collections.Generic;

using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.Repositories
{
    public interface IScopeRepository
    {
        bool InsertScope(Scope scope);

        Scope GetScopeByName(string name);

        IList<Scope> GetAllScopes();
    }
}
