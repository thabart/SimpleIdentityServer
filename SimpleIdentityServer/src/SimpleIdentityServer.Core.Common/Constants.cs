#region copyright
// Copyright 2016 Habart Thierry
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

namespace SimpleIdentityServer.Core.Common
{
    public static class ClientNames
    {
        public const string RedirectUris = "redirect_uris";
        public const string ResponseTypes = "response_types";
        public const string GrantTypes = "grant_types";
        public const string ApplicationType = "application_type";
        public const string Contacts = "contacts";
        public const string ClientName = "client_name";
        public const string LogoUri = "logo_uri";
        public const string ClientUri = "client_uri";
        public const string PolicyUri = "policy_uri";
        public const string TosUri = "tos_uri";
        public const string JwksUri = "jwks_uri";
        public const string Jwks = "jwks";
        public const string SectorIdentifierUri = "sector_identifier_uri";
        public const string SubjectType = "subject_type";
        public const string IdTokenSignedResponseAlg = "id_token_signed_response_alg";
        public const string IdTokenEncryptedResponseAlg = "id_token_encrypted_response_alg";
        public const string IdTokenEncryptedResponseEnc = "id_token_encrypted_response_enc";
        public const string UserInfoSignedResponseAlg = "userinfo_signed_response_alg";
        public const string UserInfoEncryptedResponseAlg = "userinfo_encrypted_response_alg";
        public const string UserInfoEncryptedResponseEnc = "userinfo_encrypted_response_enc";
        public const string RequestObjectSigningAlg = "request_object_signing_alg";
        public const string RequestObjectEncryptionAlg = "request_object_encryption_alg";
        public const string RequestObjectEncryptionEnc = "request_object_encryption_enc";
        public const string TokenEndpointAuthMethod = "token_endpoint_auth_method";
        public const string TokenEndpointAuthSigningAlg = "token_endpoint_auth_signing_alg";
        public const string DefaultMaxAge = "default_max_age";
        public const string RequireAuthTime = "require_auth_time";
        public const string DefaultAcrValues = "default_acr_values";
        public const string InitiateLoginUri = "initiate_login_uri";
        public const string RequestUris = "request_uris";
        public const string ScimProfile = "scim_profile";
    }

    public static class JwsProtectedHeaderNames
    {
        public const string Type = "typ";
        public const string Alg = "alg";
        public const string Kid = "kid";
    }

    public static class ClientAssertionTypes
    {
        public static string JwtBearer = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
    }
    
    public static class IntrospectionNames
    {
        public const string Active = "active";
        public const string Scope = "scope";
        public const string ClientId = "client_id";
        public const string UserName = "username";
        public const string TokenType = "token_type";
        public const string Expiration = "exp";
        public const string IssuedAt = "iat";
        public const string Nbf = "nbf";
        public const string Subject = "sub";
        public const string Audience = "aud";
        public const string Issuer = "iss";
        public const string Jti = "jti";
    }

    public static class IntrospectionRequestNames
    {
        public const string Token = "token";
        public const string TokenTypeHint = "token_type_hint";        
    }

    public static class ClientAuthNames
    {
        public const string ClientId = "client_id";
        public const string ClientSecret = "client_secret";
        public const string ClientAssertion = "client_assertion";
        public const string ClientAssertionType = "client_assertion_type";
    }

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
        public const string IntrospectionEndPoint = "introspection_endpoint";
        public const string ScopesSupported = "scopes_supported";
        public const string SubjectTypesSupported = "subject_types_supported";
        public const string TokenEndPoint = "token_endpoint";
        public const string TokenEndpointAuthMethodSupported = "token_endpoint_auth_methods_supported";
        public const string UserInfoEndPoint = "userinfo_endpoint";
        public const string Version = "version";
        public const string RegistrationEndPoint = "registration_endpoint";
        public const string ScimEndpoint = "scim_endpoint";
    }

    public static class RequestTokenNames
    {
        public const string GrantType = "grant_type";
        public const string Username = "username";
        public const string Password = "password";
        public const string Scope = "scope";
        public const string Code = "code";
        public const string RedirectUri = "redirect_uri";
        public const string RefreshToken = "refresh_token";
    }

    public static class GrantTypes
    {
        public const string ClientCredentials = "client_credentials";
        public const string Password = "password";
        public const string RefreshToken = "refresh_token";
    }

    public static class TokenTypes
    {
        public const string AccessToken = "access_token";
        public const string RefreshToken = "refresh_token";
    }

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
