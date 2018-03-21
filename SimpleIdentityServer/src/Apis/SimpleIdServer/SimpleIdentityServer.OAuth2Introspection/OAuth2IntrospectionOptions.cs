using Microsoft.AspNetCore.Authentication;
using System.Net.Http;

namespace SimpleIdentityServer.OAuth2Introspection
{
    public class OAuth2IntrospectionOptions : AuthenticationSchemeOptions
   {
        public const string AuthenticationScheme = "OAuth2Introspection";

        public string WellKnownConfigurationUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public HttpClientHandler BackChannelHttpHandler { get; set; }
    }
}
