using System.Collections.Generic;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Api
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
            }
        };

        public static class EndPoints
        {
            public static string DiscoveryAction = ".well-known/openid-configuration";

            public static string Authorization = "authorization";

            public static string Token = "token";

            public static string UserInfo = "userinfo";

            public static string Jwks = "jwks";

            public static string Registration = "registration";

            public static string EndSession = "end_session";

            public static string CheckSession = "check_session";

            public static string Revocation = "revocation";
        }
    }
}