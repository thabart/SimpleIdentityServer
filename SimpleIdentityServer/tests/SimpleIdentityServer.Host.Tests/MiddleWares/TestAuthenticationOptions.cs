using Microsoft.AspNetCore.Builder;
using System.Security.Claims;

namespace SimpleIdentityServer.Host.Tests.MiddleWares
{
    public class TestAuthenticationOptions : AuthenticationOptions
    {
        public const string TestingCookieAuthentication = "TestCookieAuthentication";
        public virtual ClaimsIdentity Identity { get; } = new ClaimsIdentity(new Claim[]
        {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "administrator")
        }, TestingCookieAuthentication);

        public TestAuthenticationOptions()
        {
            this.AuthenticationScheme = TestingCookieAuthentication;
            this.AutomaticAuthenticate = true;
        }
    }
}
