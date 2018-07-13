using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleIdentityServer.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Tests.MiddleWares
{
    public class UserStore
    {
        private static UserStore _instance;
        private static string _defaultSubject = "administrator";

        private UserStore()
        {
            Subject = _defaultSubject;
        }

        public static UserStore Instance()
        {
            if (_instance == null)
            {
                _instance = new UserStore();
            }

            return _instance;
        }

        public bool IsInactive { get; set; }
        public string Subject { get; set; }
        public DateTimeOffset? AuthenticationOffset { get; set; }
    }

    public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationOptions>
    {
        public TestAuthenticationHandler(IOptionsMonitor<TestAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (UserStore.Instance().IsInactive)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim>();
            claims.Add(new Claim("sub", UserStore.Instance().Subject));
            if (UserStore.Instance().AuthenticationOffset != null)
            {
                claims.Add(new Claim(ClaimTypes.AuthenticationInstant, UserStore.Instance().AuthenticationOffset.Value.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture)));

            }

            var claimsIdentity = new ClaimsIdentity(claims, FakeStartup.DefaultSchema);
            var authenticationTicket = new AuthenticationTicket(
                                             new ClaimsPrincipal(claimsIdentity),
                                             new Microsoft.AspNetCore.Authentication.AuthenticationProperties(),
                                             FakeStartup.DefaultSchema);
            return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
        }
    }
}
