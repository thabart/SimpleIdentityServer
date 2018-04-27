using Microsoft.AspNetCore.Authentication;
using System;

namespace SimpleIdentityServer.UserInfoIntrospection
{
    public static class OAuth2IntrospectionExtensions
    {
        public static AuthenticationBuilder AddUserInfoIntrospection(this AuthenticationBuilder builder)
            => builder.AddUserInfoIntrospection(UserInfoIntrospectionOptions.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddUserInfoIntrospection(this AuthenticationBuilder builder, Action<UserInfoIntrospectionOptions> configureOptions)
            => builder.AddUserInfoIntrospection(UserInfoIntrospectionOptions.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddUserInfoIntrospection(this AuthenticationBuilder builder, string authenticationScheme, Action<UserInfoIntrospectionOptions> configureOptions)
            => builder.AddScheme<UserInfoIntrospectionOptions, UserInfoIntrospectionHandler>(authenticationScheme, configureOptions);
    }
}
