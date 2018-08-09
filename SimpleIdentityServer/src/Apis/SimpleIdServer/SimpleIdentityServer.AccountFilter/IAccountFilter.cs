using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccountFilter
{
    public interface IAccountFilter
    {
        Task<AccountFilterResult> Check(IEnumerable<Claim> claims);
    }
}
