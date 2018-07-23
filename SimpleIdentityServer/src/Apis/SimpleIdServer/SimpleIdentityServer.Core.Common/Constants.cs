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
    public static class ErrorResponseWithStateNames
    {
        public const string State = "state";
    }

    public static class StandardClaimNames
    {
        public static string Issuer = "iss";
        public static string Audiences = "aud";
        public static string ExpirationTime = "exp";
        public static string Iat = "iat";
        public static string AuthenticationTime = "auth_time";
        public static string Nonce = "nonce";
        public static string Acr = "acr";
        public static string Amr = "amr";
        public static string Azp = "azp";
        /// <summary>
        /// Unique identifier of the JWT.
        /// </summary>
        public static string Jti = "jti";
        /// <summary>
        /// Access token hash value
        /// </summary>
        public static string AtHash = "at_hash";
        /// <summary>
        /// Authorization code hash value
        /// </summary>
        public static string CHash = "c_hash";
        public static string ClientId = "client_id";
        public static string Scopes = "scope";
    }


    public static class GrantTypes
    {
        public const string ClientCredentials = "client_credentials";
        public const string Password = "password";
        public const string RefreshToken = "refresh_token";
        public const string AuthorizationCode = "authorization_code";
        public const string ValidateBearer = "validate_bearer";
        public const string UmaTicket = "uma_ticket";
    }

    public static class TokenTypes
    {
        public const string AccessToken = "access_token";
        public const string RefreshToken = "refresh_token";
    }

    public static class PromptNames
    {
        public const string None = "none";
        public const string Login = "login";
        public const string Consent = "consent";
        public const string SelectAccount = "select_account";
    }
    public static class ClientAssertionTypes
    {
        public static string JwtBearer = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
    }

    // https://docs.kantarainitiative.org/uma/wg/oauth-uma-grant-2.0-05.html#seek-authorization
    public static class RequestTokenUma
    {
        public const string Ticket = "ticket";
        public const string ClaimToken = "claim_token";
        public const string ClaimTokenFormat = "claim_token_format";
        public const string Pct = "pct";
        public const string Rpt = "rpt";
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

    public static class ResponseModeNames
    {
        public const string None = "none";
        public const string Query = "query";
        public const string Fragment = "fragment";
        public const string FormPost = "form_post";
    }

    public static class ResponseTypeNames
    {
        public const string Code = "code";
        public const string Token = "token";
        public const string IdToken = "id_token";
    }

    public static class PageNames
    {
        public const string Page = "page";
        public const string Popup = "popup";
        public const string Touch = "touch";
        public const string Wap = "wap";
    }

    public static class CodeChallenges
    {
        public const string Plain = "plain";
        public const string S256 = "S256";
    }

    public static class RequestAuthorizationCodeNames
    {
        public const string Scope = "scope";
        public const string ResponseType = "response_type";
        public const string RedirectUri = "redirect_uri";
        public const string State = "state";
        public const string ResponseMode = "response_mode";
        public const string Nonce = "nonce";
        public const string Display = "display";
        public const string Prompt = "prompt";
        public const string MaxAge = "max_age";
        public const string UiLocales = "ui_locales";
        public const string IdTokenHint = "id_token_hint";
        public const string LoginHint = "login_hint";
        public const string Claims = "claims";
        public const string AcrValues = "acr_values";
        public const string Request = "request";
        public const string RequestUri = "request_uri";
        public const string CodeChallenge = "code_challenge";
        public const string CodeChallengeMethod = "code_challenge_method";
    }

    public static class ClientAuthNames
    {
        public const string ClientId = "client_id";
        public const string ClientSecret = "client_secret";
        public const string ClientAssertion = "client_assertion";
        public const string ClientAssertionType = "client_assertion_type";
    }

    public static class EventResponseNames
    {
        public const string Id = "id";
        public const string AggregateId = "aggregate_id";
        public const string Payload = "payload";
        public const string Description = "description";
        public const string Order = "order";
        public const string CreatedOn = "created_on";
    }

    public static class RegistrationResponseNames
    {
        public const string RegistrationAccessToken = "registration_access_token";
        public const string RegistrationClientUri = "registration_client_uri";
        public const string ClientIdIssuedAt = "client_id_issued_at";
        public const string ClientSecretExpiresAt = "client_secret_expires_at";
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

    public static class JwsProtectedHeaderNames
    {
        public const string Type = "typ";
        public const string Alg = "alg";
        public const string Kid = "kid";
    }

    public static class RevocationRequestNames
    {
        public const string Token = "token";
        public const string TokenTypeHint = "token_type_hint";
    }

    public static class RevokeSessionRequestNames
    {
        public const string IdTokenHint = "id_token_hint";
        public const string PostLogoutRedirectUri = "post_logout_redirect_uri";
        public const string State = "state";
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
        public const string CodeVerifier = "code_verifier";
        public const string AmrValues = "amr_values";
    }
}
