#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.Cookies;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Host
{
    public static class Constants
    {
        public static class CookieNames
        {
            public const string CookieName = CookieAuthenticationDefaults.AuthenticationScheme;
            public const string ExternalCookieName = "SimpleIdServer-OpenId-External";
            public const string PasswordLessCookieName = "SimpleIdServer-PasswordLess";
            public const string TwoFactorCookieName = "SimpleIdentityServer-TwoFactorAuth";
        }

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
            public const string RevokeSessionCallback = "revoke_session_callback";
            public const string Revocation = "token/revoke";
            public const string Introspection = "introspect";
            public const string CheckSession = "check_session";
            public const string EndSession = "end_session";
            public const string EndSessionCallback = "end_session_callback";
            public const string Get401 = "Error/401";            
            public const string Get404 = "Error/404";
            public const string Get500 = "Error/500";
        }
    }
}