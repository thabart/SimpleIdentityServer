using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Tests.MiddleWares
{
    public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationOptions>
    {
        public TestAuthenticationHandler(IOptionsMonitor<TestAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new List<Claim>();
            claims.Add(new Claim("sub", "administrator"));
            var claimsIdentity = new ClaimsIdentity(claims, FakeStartup.DefaultSchema);
            var authenticationTicket = new AuthenticationTicket(
                                             new ClaimsPrincipal(claimsIdentity),
                                             new Microsoft.AspNetCore.Authentication.AuthenticationProperties(),
                                             FakeStartup.DefaultSchema);
            return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
        }
    }
}
