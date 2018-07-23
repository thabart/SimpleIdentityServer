using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleIdentityServer.Client;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SimpleIdentityServer.UserInfoIntrospection
{
    public class UserInfoIntrospectionHandler : AuthenticationHandler<UserInfoIntrospectionOptions>
    {
        public UserInfoIntrospectionHandler(IOptionsMonitor<UserInfoIntrospectionOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authorization = Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authorization))
            {
                return AuthenticateResult.NoResult();
            }

            string token = null;
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorization.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.NoResult();
            }

            var factory = new IdentityServerClientFactory();
            try
            {
                var introspectionResult = await factory.CreateUserInfoClient()
                    .Resolve(Options.WellKnownConfigurationUrl, token)
                    .ConfigureAwait(false);
                if (introspectionResult == null || introspectionResult.ContainsError)
                {
                    return AuthenticateResult.NoResult();
                }

                var claims = new List<Claim>();
                var values = introspectionResult.Content.ToObject<Dictionary<string, object>>();
                foreach(var kvp in values)
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
                }
                var claimsIdentity = new ClaimsIdentity(claims, UserInfoIntrospectionOptions.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                var authenticationTicket = new AuthenticationTicket(
                                                 claimsPrincipal,
                                                 new AuthenticationProperties(),
                                                 UserInfoIntrospectionOptions.AuthenticationScheme);
                return AuthenticateResult.Success(authenticationTicket);
            }
            catch(Exception)
            {
                return AuthenticateResult.NoResult();
            }
        }
    }
}
