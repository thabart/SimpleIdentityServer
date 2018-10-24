using Microsoft.AspNetCore.Authentication;
using SimpleIdentityServer.Common.Client.Factories;
using System.Net.Http;

namespace SimpleIdentityServer.UserInfoIntrospection
{
    public class UserInfoIntrospectionOptions : AuthenticationSchemeOptions
   {
        public const string AuthenticationScheme = "UserInfoIntrospection";

        public string WellKnownConfigurationUrl { get; set; }
        public IHttpClientFactory HttpClientFactory { get; set; }
    }
}
