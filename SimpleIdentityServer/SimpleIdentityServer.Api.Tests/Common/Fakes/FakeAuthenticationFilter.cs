using System.Collections.Generic;
using System.Security.Claims;

using System.Threading;
using System.Threading.Tasks;

using System.Web.Http.Filters;

namespace SimpleIdentityServer.Api.Tests.Common.Fakes
{
    public class FakeAuthenticationFilter : IAuthenticationFilter
    {
        public string ResourceOwnerId { get; set; }

        public string ResourceOwnerUserName { get; set; }

        public bool AllowMultiple { get; private set; }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            if (ResourceOwnerId == null || ResourceOwnerUserName == null)
            {
                return Task.FromResult(0);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, ResourceOwnerId),
                new Claim(ClaimTypes.Name, ResourceOwnerUserName)
            };
            var identity = new ClaimsIdentity(claims, "FakeApi");
            context.Principal = new ClaimsPrincipal(identity);
            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
