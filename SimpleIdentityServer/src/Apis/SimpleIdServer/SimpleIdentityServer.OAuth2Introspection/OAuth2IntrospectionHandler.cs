using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleIdentityServer.Client;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SimpleIdentityServer.OAuth2Introspection
{
    public class OAuth2IntrospectionHandler : AuthenticationHandler<OAuth2IntrospectionOptions>
    {
        public OAuth2IntrospectionHandler(IOptionsMonitor<OAuth2IntrospectionOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
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
            var introspectionResult = await factory.CreateAuthSelector()
                .UseClientSecretPostAuth(Options.ClientId, Options.ClientSecret)
                .Introspect(token, TokenType.AccessToken)
                .ResolveAsync(Options.WellKnownConfigurationUrl);
            if (!introspectionResult.Active)
            {
                return AuthenticateResult.NoResult();
            }

            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, introspectionResult.Subject),
                new Claim(Core.Jwt.Constants.StandardClaimNames.ClientId, introspectionResult.ClientId),
                new Claim(Core.Jwt.Constants.StandardClaimNames.ExpirationTime, introspectionResult.Expiration.ToString()),
                new Claim(Core.Jwt.Constants.StandardClaimNames.Iat, introspectionResult.IssuedAt.ToString()),
                new Claim(Core.Jwt.Constants.StandardClaimNames.Issuer, introspectionResult.Issuer)
            };
            if (introspectionResult.Scope != null)
            {
                foreach(var scope in introspectionResult.Scope)
                {
                    claims.Add(new Claim(Core.Jwt.Constants.StandardClaimNames.Scopes, scope));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, OAuth2IntrospectionOptions.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var authenticationTicket = new AuthenticationTicket(
                                             claimsPrincipal,
                                             new AuthenticationProperties(),
                                             OAuth2IntrospectionOptions.AuthenticationScheme);
            return AuthenticateResult.Success(authenticationTicket);
        }
    }
}
