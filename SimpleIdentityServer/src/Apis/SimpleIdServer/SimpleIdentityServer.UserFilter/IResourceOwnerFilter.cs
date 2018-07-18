using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.UserFilter
{
    public interface IResourceOwnerFilter
    {
        UserFilterResult Check(IEnumerable<Claim> claims);
    }
}
