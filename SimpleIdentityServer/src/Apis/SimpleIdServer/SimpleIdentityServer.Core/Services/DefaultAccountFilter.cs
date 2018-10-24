using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SimpleIdentityServer.AccountFilter;

namespace SimpleIdentityServer.Core.Services
{
    internal sealed class DefaultAccountFilter : IAccountFilter
    {
        public Task<AccountFilterResult> Check(IEnumerable<Claim> claims)
        {
            return Task.FromResult(new AccountFilterResult
            {
                IsValid = true
            });
        }
    }
}
