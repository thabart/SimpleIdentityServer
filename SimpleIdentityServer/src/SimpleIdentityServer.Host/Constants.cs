using System.Collections.Generic;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Host
{
    public static class Constants
    {
        public static Dictionary<IdentityServerEndPoints, string> MappingIdentityServerEndPointToPartialUrl = new Dictionary<IdentityServerEndPoints, string>
        {
            {
                IdentityServerEndPoints.AuthenticateIndex,
                "/Authenticate/OpenId"
            },
            {
                IdentityServerEndPoints.ConsentIndex,
                "/Consent"
            },
            {
                IdentityServerEndPoints.FormIndex,
                "/Form"
            }
        };

        public static class EndPoints
        {
            public const string DiscoveryAction = ".well-known/openid-configuration";

            public const string Authorization = "authorization";

            public const string Token = "token";

            public const string UserInfo = "userinfo";

            public const string Jwks = "jwks";

            public const string Registration = "registration";

            public const string EndSession = "end_session";

            public const string CheckSession = "check_session";

            public const string Revocation = "token/revoke";

            public const string Introspection = "introspect";

            public const string Get401 = "Error/401";
            
            public const string Get404 = "Error/404";
        }
        
        public static class IdentityProviderNames 
        {
            public const string Microsoft = "Microsoft";
            
            public const string Facebook = "Facebook";
        }
        
        public static List<string> SupportedIdentityProviders = new List<string> 
        {
            IdentityProviderNames.Microsoft,
            IdentityProviderNames.Facebook
        };

        public static class RevocationRequestNames
        {
            public const string Token = "token";

            public const string TokenTypeHint = "token_type_hint";

            public const string ClientId = "client_id";

            public const string ClientSecret = "client_secret";

            public const string ClientAssertionType = "client_assertion_type";

            public const string ClientAssertion = "client_assertion";
        }
    }
}