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

namespace SimpleIdentityServer.Client
{
    internal static class Constants
    {
        public static class DiscoveryInformationNames
        {
            public const string AuthorizationEndPoint = "authorization_endpoint";
            public const string CheckSessionEndPoint = "check_session_iframe";
            public const string ClaimsParameterSupported = "claims_parameter_supported";
            public const string ClaimsSupported = "claims_supported";
            public const string EndSessionEndPoint = "end_session_endpoint";
            public const string GrantTypesSupported = "grant_types_supported";
            public const string IdTokenSigningAlgValuesSupported = "id_token_signing_alg_values_supported";
            public const string Issuer = "issuer";
            public const string JwksUri = "jwks_uri";
            public const string RequestParameterSupported = "request_parameter_supported";
            public const string RequestUriParameterSupported = "request_uri_parameter_supported";
            public const string RequireRequestUriRegistration = "require_request_uri_registration";
            public const string ResponseModesSupported = "response_modes_supported";
            public const string ResponseTypesSupported = "response_types_supported";
            public const string RevocationEndPoint = "revocation_endpoint";
            public const string ScopesSupported = "scopes_supported";
            public const string SubjectTypesSupported = "subject_types_supported";
            public const string TokenEndPoint = "token_endpoint";
            public const string TokenEndpointAuthMethodSupported = "token_endpoint_auth_methods_supported";
            public const string UserInfoEndPoint = "userinfo_endpoint";
            public const string Version = "version";
            public const string RegistrationEndPoint = "registration_endpoint";
            public const string ScimEndPoint = "scim_endpoint";
        }

        public static class GrantedTokenNames
        {
            public const string AccessToken = "access_token";

            public const string IdToken = "id_token";

            public const string TokenType = "token_type";

            public const string RefreshToken = "refresh_token";

            public const string ExpiresIn = "expires_in";

            public const string Scope = "scope";
        }

        public static class TokenRequestNames
        {
            public const string GrantType = "grant_type";

            public const string Username = "username";

            public const string Password = "password";

            public const string Scope = "scope";

            public const string ClientId = "client_id";

            public const string ClientSecret = "client_secret";

            public const string Code = "code";

            public const string RedirectUri = "redirect_uri";

            public const string ClientAssertionType = "client_assertion_type";

            public const string ClientAssertion = "client_assertion";

            public const string RefreshToken = "refresh_token";
        }
    }
}
