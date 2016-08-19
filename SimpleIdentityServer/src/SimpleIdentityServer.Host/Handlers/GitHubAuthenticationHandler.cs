using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Handlers
{
    public static class GitHubAuthenticationDefaults
    {
        /// <summary>
        /// Default value for <see cref="AuthenticationOptions.AuthenticationScheme"/>.
        /// </summary>
        public const string AuthenticationScheme = "GitHub";

        /// <summary>
        /// Default value for <see cref="RemoteAuthenticationOptions.DisplayName"/>.
        /// </summary>
        public const string DisplayName = "GitHub";

        /// <summary>
        /// Default value for <see cref="AuthenticationOptions.ClaimsIssuer"/>.
        /// </summary>
        public const string Issuer = "GitHub";

        /// <summary>
        /// Default value for <see cref="RemoteAuthenticationOptions.CallbackPath"/>.
        /// </summary>
        public const string CallbackPath = "/signin-github";

        /// <summary>
        /// Default value for <see cref="OAuthOptions.AuthorizationEndpoint"/>.
        /// </summary>
        public const string AuthorizationEndpoint = "https://github.com/login/oauth/authorize";

        /// <summary>
        /// Default value for <see cref="OAuthOptions.TokenEndpoint"/>.
        /// </summary>
        public const string TokenEndpoint = "https://github.com/login/oauth/access_token";

        /// <summary>
        /// Default value for <see cref="OAuthOptions.UserInformationEndpoint"/>.
        /// </summary>
        public const string UserInformationEndpoint = "https://api.github.com/user";

        /// <summary>
        /// Default value for <see cref="GitHubAuthenticationOptions.UserEmailsEndpoint"/>.
        /// </summary>
        public const string UserEmailsEndpoint = "https://api.github.com/user/emails";
    }

    public class GitHubAuthenticationOptions : OAuthOptions
    {
        public GitHubAuthenticationOptions()
        {
            AuthenticationScheme = GitHubAuthenticationDefaults.AuthenticationScheme;
            DisplayName = GitHubAuthenticationDefaults.DisplayName;
            ClaimsIssuer = GitHubAuthenticationDefaults.Issuer;
            CallbackPath = new PathString(GitHubAuthenticationDefaults.CallbackPath);
            AuthorizationEndpoint = GitHubAuthenticationDefaults.AuthorizationEndpoint;
            TokenEndpoint = GitHubAuthenticationDefaults.TokenEndpoint;
            UserInformationEndpoint = GitHubAuthenticationDefaults.UserInformationEndpoint;
        }

        /// <summary>
        /// Gets or sets the address of the endpoint exposing
        /// the email addresses associated with the logged in user.
        /// </summary>
        public string UserEmailsEndpoint { get; } = GitHubAuthenticationDefaults.UserEmailsEndpoint;
    }

    public static class GitHubAuthenticationHelper
    {
        /// <summary>
        /// Gets the identifier corresponding to the authenticated user.
        /// </summary>
        public static string GetIdentifier(JObject user) => user.Value<string>("id");

        /// <summary>
        /// Gets the login corresponding to the authenticated user.
        /// </summary>
        public static string GetLogin(JObject user) => user.Value<string>("login");

        /// <summary>
        /// Gets the email address corresponding to the authenticated user.
        /// </summary>
        public static string GetEmail(JObject user) => user.Value<string>("email");

        /// <summary>
        /// Gets the primary email address contained in the given array.
        /// </summary>
        public static string GetEmail(JArray array)
        {
            return (from address in array.AsJEnumerable()
                    where address.Value<bool>("primary")
                    select address.Value<string>("email")).FirstOrDefault();
        }

        /// <summary>
        /// Gets the name corresponding to the authenticated user.
        /// </summary>
        public static string GetName(JObject user) => user.Value<string>("name");

        /// <summary>
        /// Gets the URL corresponding to the authenticated user.
        /// </summary>
        public static string GetLink(JObject user) => user.Value<string>("url");
    }

    public class GitHubAuthenticationHandler : OAuthHandler<GitHubAuthenticationOptions>
    {
        public GitHubAuthenticationHandler(HttpClient client)
            : base(client)
        {
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(
            ClaimsIdentity identity,
            AuthenticationProperties properties, 
            OAuthTokenResponse tokens)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Options.UserInformationEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            var response = await Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                var s = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException("An error occurred when retrieving the user profile.");
            }

            var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, GitHubAuthenticationHelper.GetIdentifier(payload), Options.ClaimsIssuer));
            identity.AddClaim(new Claim(ClaimTypes.Name, GitHubAuthenticationHelper.GetLogin(payload), Options.ClaimsIssuer));
            identity.AddClaim(new Claim(ClaimTypes.Email, GitHubAuthenticationHelper.GetEmail(payload), Options.ClaimsIssuer));
            identity.AddClaim(new Claim("urn:github:name", GitHubAuthenticationHelper.GetName(payload), Options.ClaimsIssuer));
            identity.AddClaim(new Claim("urn:github:url", GitHubAuthenticationHelper.GetLink(payload), Options.ClaimsIssuer));

            // When the email address is not public, retrieve it from
            // the emails endpoint if the user:email scope is specified.
            if (!string.IsNullOrEmpty(Options.UserEmailsEndpoint) &&
                !identity.HasClaim(claim => claim.Type == ClaimTypes.Email) && Options.Scope.Contains("user:email"))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, await GetEmailAsync(tokens), Options.ClaimsIssuer));
            }

            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, properties, Options.AuthenticationScheme);

            var context = new OAuthCreatingTicketContext(ticket, Context, Options, Backchannel, tokens, payload);
            await Options.Events.CreatingTicket(context);

            return context.Ticket;
        }

        protected virtual async Task<string> GetEmailAsync(OAuthTokenResponse tokens)
        {
            // See https://developer.github.com/v3/users/emails/ for more information about the /user/emails endpoint.
            var request = new HttpRequestMessage(HttpMethod.Get, Options.UserEmailsEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            // Failed requests shouldn't cause an error: in this case, return null to indicate that the email address cannot be retrieved.
            var response = await Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var payload = JArray.Parse(await response.Content.ReadAsStringAsync());

            return GitHubAuthenticationHelper.GetEmail(payload);
        }
    }
}
