using Microsoft.AspNetCore.Authentication;
using System;

namespace SimpleIdentityServer.OAuth2Introspection
{
    public static class OAuth2IntrospectionExtensions
    {
        public static AuthenticationBuilder AddOAuth2Introspection(this AuthenticationBuilder builder)
            => builder.AddOAuth2Introspection(OAuth2IntrospectionOptions.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddOAuth2Introspection(this AuthenticationBuilder builder, Action<OAuth2IntrospectionOptions> configureOptions)
            => builder.AddOAuth2Introspection(OAuth2IntrospectionOptions.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddOAuth2Introspection(this AuthenticationBuilder builder, string authenticationScheme, Action<OAuth2IntrospectionOptions> configureOptions)
            => builder.AddScheme<OAuth2IntrospectionOptions, OAuth2IntrospectionHandler>(authenticationScheme, configureOptions);
    }
}
