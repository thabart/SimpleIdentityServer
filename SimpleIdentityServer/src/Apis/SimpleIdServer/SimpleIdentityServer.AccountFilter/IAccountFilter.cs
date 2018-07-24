using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.AccountFilter
{
    public interface IAccountFilter
    {
        AccountFilterResult Check(IEnumerable<Claim> claims);
    }
}
