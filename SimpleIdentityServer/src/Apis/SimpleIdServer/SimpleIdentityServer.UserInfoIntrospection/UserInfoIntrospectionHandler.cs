using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Core.Jwt.Extensions;
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

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authorization = Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authorization))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            string token = null;
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorization.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            return HandleAuthenticate(Options.HttpClientFactory, Options.WellKnownConfigurationUrl, token);
        }
        
        internal static async Task<AuthenticateResult> HandleAuthenticate(IHttpClientFactory httpClientFactory, string wellKnownConfiguration, string token)
        {
            var factory = new IdentityServerClientFactory(httpClientFactory);
            try
            {
                var introspectionResult = await factory.CreateUserInfoClient()
                    .Resolve(wellKnownConfiguration, token)
                    .ConfigureAwait(false);
                if (introspectionResult == null || introspectionResult.ContainsError)
                {
                    return AuthenticateResult.NoResult();
                }

                var claims = new List<Claim>();
                var values = introspectionResult.Content.ToObject<Dictionary<string, object>>();
                foreach (var kvp in values)
                {
                    claims.AddRange(Convert(kvp));
                }

                var claimsIdentity = new ClaimsIdentity(claims, UserInfoIntrospectionOptions.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                var authenticationTicket = new AuthenticationTicket(claimsPrincipal, new AuthenticationProperties(), UserInfoIntrospectionOptions.AuthenticationScheme);
                return AuthenticateResult.Success(authenticationTicket);
            }
            catch (Exception)
            {
                return AuthenticateResult.NoResult();
            }
        }

        private static IEnumerable<Claim> Convert(KeyValuePair<string, object> kvp)
        {
            var arr = kvp.Value as JArray;
            if (arr == null)
            {
                return new List<Claim>
                {
                    new Claim(kvp.Key, kvp.Value.ToString())
                };
            }

            var result = new List<Claim>();
            foreach(var r in arr)
            {
                result.Add(new Claim(kvp.Key, r.ToString()));
            }

            return result.ToClaims();
        }
    }
}