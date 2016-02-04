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
                "/Authenticate"
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

            public const string Revocation = "revocation";
            
            public const string Get401 = "Error/401";
        }
    }
}