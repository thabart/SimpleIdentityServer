using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Tests.MiddleWares
{
    public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationOptions>
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authenticationTicket = new AuthenticationTicket(
                                             new ClaimsPrincipal(Options.Identity),
                                             new AuthenticationProperties(),
                                             this.Options.AuthenticationScheme);
            return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
        }
    }
}
