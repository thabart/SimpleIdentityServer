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
    public static class Constants
    {
        public static class PostPermissionNames
        {
            public const string ResourceSetId = "resource_set_id";

            public const string Scopes = "scopes";
        }

        public static class AddPolicyResponseNames
        {
            public const string PolicyId = "policy";
        }

        public static class PolicyNames
        {
            public const string Id = "id";

            public const string ClientIdsAllowed = "allowed_clients";

            public const string Scopes = "scopes";

            public const string IsResourceOwnerConsentNeeded = "consent_needed";

            public const string IsCustom = "is_custom";

            public const string Script = "script";

            public const string ResourceSetIds = "resource_set_ids";

            public const string Claims = "claims";
        }

        public static class AddPermissionResponseNames
        {
            public const string TicketId = "ticket_id";
        }

        public static class AuthorizationResponseNames
        {
            public const string Rpt = "rpt";
        }

        public static class PostClaimTokenNames
        {
            public const string Format = "format";

            public const string Token = "token";
        }

        public static class PostAuthorizationNames
        {
            public const string Rpt = "rpt";

            public const string TicketId = "ticket";

            public const string ClaimTokens = "claim_tokens";
        }

        public static class ConfigurationResponseNames
        {
            public const string Version = "version";

            public const string Issuer = "issuer";

            public const string PatProfilesSupported = "pat_profiles_supported";

            public const string AatProfilesSupported = "aat_profiles_supported";

            public const string RtpProfilesSupported = "rpt_profiles_supported";

            public const string PatGrantTypesSupported = "pat_grant_types_supported";

            public const string AatGrantTypesSupported = "aat_grant_types_supported";

            public const string ClaimTokenProfilesSupported = "claim_token_profiles_supported";

            public const string UmaProfilesSupported = "uma_profiles_supported";

            public const string DynamicClientEndPoint = "dynamic_client_endpoint";

            public const string TokenEndPoint = "token_endpoint";

            public const string AuthorizationEndPoint = "authorization_endpoint";

            public const string RequestingPartyClaimsEndPoint = "requesting_party_claims_endpoint";

            public const string IntrospectionEndPoint = "introspection_endpoint";

            public const string ResourceSetRegistrationEndPoint = "resource_set_registration_endpoint";

            public const string PermissionRegistrationEndPoint = "permission_registration_endpoint";

            public const string RptEndPoint = "rpt_endpoint";

            public const string PolicyEndPoint = "policy_endpoint";
        }

        public static class ClaimNames
        {
            public const string Type = "type";

            public const string Value = "value";
        }

        public static class PostPolicyNames
        {
            public const string ClientIdsAllowed = "allowed_clients";

            public const string Scopes = "scopes";

            public const string IsResourceOwnerConsentNeeded = "consent_needed";

            public const string Script = "script";

            public const string ResourceSetIds = "resource_set_ids";
        }

        public static class ResourceSetResponseNames
        {
            public const string Id = "_id";

            public const string Name = "name";

            public const string Uri = "uri";

            public const string Type = "type";

            public const string Scopes = "scopes";

            public const string IconUri = "icon_uri";
        }

        public static class AddResourceSetResponseNames
        {
            public const string Id = "_id";

            public const string UserAccessPolicyUri = "user_access_policy_uri";
        }
    }
}
