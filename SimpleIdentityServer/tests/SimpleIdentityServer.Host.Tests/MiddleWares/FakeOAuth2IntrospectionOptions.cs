using Microsoft.AspNetCore.Authentication;
using SimpleIdentityServer.Client;

namespace SimpleIdentityServer.Host.Tests.MiddleWares
{
    public class FakeOAuth2IntrospectionOptions : AuthenticationSchemeOptions
    {
        public FakeOAuth2IntrospectionOptions()
        {
            IdentityServerClientFactory = new IdentityServerClientFactory();
        }

        public const string AuthenticationScheme = "OAuth2Introspection";

        public string WellKnownConfigurationUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public IIdentityServerClientFactory IdentityServerClientFactory { get; set; }
    }
}
