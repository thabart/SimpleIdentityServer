using Microsoft.AspNetCore.Authentication;
using System.Net.Http;

namespace SimpleIdentityServer.UserInfoIntrospection
{
    public class UserInfoIntrospectionOptions : AuthenticationSchemeOptions
   {
        public const string AuthenticationScheme = "UserInfoIntrospection";

        public string WellKnownConfigurationUrl { get; set; }
        public HttpClientHandler BackChannelHttpHandler { get; set; }
    }
}
