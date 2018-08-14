using Microsoft.AspNetCore.Authentication;

namespace SimpleIdentityServer.Host.Tests.MiddleWares
{
    public class FakeUserInfoIntrospectionOptions : AuthenticationSchemeOptions
    {
        public const string AuthenticationScheme = "UserInfoIntrospection";
    }
}
