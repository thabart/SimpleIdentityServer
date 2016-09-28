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

namespace SimpleIdentityServer.Manager.Host
{
    public static class Constants
    {
        public static class EndPoints
        {
            public const string RootPath = "/api";
            public const string Jws = RootPath + "/jws";
            public const string Jwe = RootPath + "/jwe";
            public const string Clients = RootPath + "/clients";
            public const string Scopes = RootPath + "/scopes";
            public const string ResourceOwners = RootPath + "/resource_owners";
            public const string Manage = RootPath + "/manage";
        }

        public static class GetJwsRequestNames
        {
            public const string Jws = "jws";
            public const string Url = "url";
        }

        public static class GetJweRequestNames
        {
            public const string Jwe = "jwe";
            public const string Url = "url";
            public const string Password = "password";
        }

        public static class JwsInformationResponseNames
        {
            public const string Header = "header";
            public const string Payload = "payload";
            public const string JsonWebKey = "jsonwebkey";
        }

        public static class JweInformationResponseNames
        {
            public const string Content = "content";
            public const string IsContentJws = "iscontentjws";
        }

        public static class ErrorResponseNames
        {
            public const string Code = "code";
            public const string Message = "message";
        }

        public static class CreateJwsRequestNames
        {
            public const string Kid = "kid";
            public const string Alg = "alg";
            public const string Url = "url";
            public const string Payload = "payload";
        }

        public static class CreateJweRequestNames
        {
            public const string Jws = "jws";
            public const string Url = "url";
            public const string Kid = "kid";
            public const string Alg = "alg";
            public const string Enc = "enc";
            public const string Password = "password";
        }

        public static class UpdateResourceOwnerRequestNames
        {
            public const string Subject = "sub";
            public const string Roles = "roles";
        }

        public static class AddResourceOwnerRequestNames
        {
            public const string Subject = "sub";
            public const string Password = "password";
        }

        public static class ResourceOwnerResponseNames
        {
            public const string Id = "id";
            public const string Name = "name";
            public const string GivenName = "given_name";
            public const string FamilyName = "family_name";
            public const string MiddleName = "middle_name";
            public const string NickName = "nick_name";
            public const string PreferredUserName = "preferred_username";
            public const string Profile = "profile";
            public const string WebSite = "website";
            public const string Email = "email";
            public const string EmailVerified = "email_verified";
            public const string Gender = "gender";
            public const string BirthDate = "birthdate";
            public const string ZoneInfo = "zone";
            public const string Locale = "locale";
            public const string PhoneNumber = "phone";
            public const string PhoneNumberVerified = "phone_verified";
            public const string UpdatedAt = "updated_at";
            public const string Password = "password";
            public const string Roles = "roles";
            public const string Picture = "picture";
            public const string IsLocalAccount = "is_localaccount";
        }

        public static class ClientNames
        {
            public const string ClientId = "client_id";
            public const string ClientSecret = "client_secret";
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
            public const string SectoreIdentifierUri = "sector_identifier_uri";
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
            public const string TokenEndPointAuthMethod = "token_endpoint_auth_method";
            public const string TokenEndPointAuthSigningAlg = "token_endpoint_auth_signing_alg";
            public const string DefaultMaxAge = "default_max_age";
            public const string RequireAuthTime = "require_auth_time";
            public const string DefaultAcrValues = "default_acr_values";
            public const string InitiateLoginUri = "initiate_login_uri";
            public const string RequestUris = "request_uris";
            public const string AllowedScopes = "allowed_scopes";
            public const string JsonWebKeys = "json_web_keys";
            public const string RedirectionUrls = "redirection_urls";
            public const string SectorIdentifierUri = "sector_identifier_uri";
        }

        public static class ScopeResponseNames
        {
            public const string Name = "name";
            public const string Description = "description";
            public const string IsDisplayedInConsent = "is_displayed_in_consent";
            public const string IsOpenIdScope = "is_openid_scope";
            public const string IsExposed = "is_exposed";
            public const string Type = "type";
            public const string Claims = "claims";
        }

        public static class ExportResponseNames
        {
            public const string Clients = "clients";
        }
    }
}
