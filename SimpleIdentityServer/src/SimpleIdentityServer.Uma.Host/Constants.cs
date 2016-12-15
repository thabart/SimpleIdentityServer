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

namespace SimpleIdentityServer.Uma.Host
{
    public static class Constants
    {
        public static class RouteValues
        {
            public const string Configuration = ".well-known/uma-configuration";
            public const string ResourceSet = "rs/resource_set";
            public const string Scope = "scopes";
            public const string Permission = "/perm";
            public const string Authorization = "/rpt";
            public const string Policies = "/policies";
            public const string Introspection = "/status";
            public const string CodeSample = "/codesamples";
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

            public const string RtpEndPoint = "rpt_endpoint";

            public const string PolicyEndPoint = "policy_endpoint";
        }

        public static class AuthorizationResponseNames
        {
            public const string Rpt = "rpt";
        }

        public static class PostAuthorizationNames
        {
            public const string Rpt = "rpt";

            public const string TicketId = "ticket";

            public const string ClaimTokens = "claim_tokens";
        }

        public static class PostClaimTokenNames
        {
            public const string Format = "format";

            public const string Token = "token";
        }

        public static class ClaimNames
        {
            public const string Type = "type";

            public const string Value = "value";
        }

        public static class ErrorResponseNames
        {
            public const string Error = "error";

            public const string ErrorDescription = "error_description";

            public const string ErrorDetails = "error_details";
        }

        public static class ScopeResponseNames
        {
            public const string Id = "id";

            public const string Name = "name";

            public const string IconUri = "icon_uri";
        }

        public static class ErrorCodes
        {
            public const string NotFound = "not_found";

            public const string UnSupportedMethodType = "unsupported_method_type";
        }

        public static class ErrorDescriptions
        {
            public const string ResourceSetNotFound = "resource cannot be found";

            public const string ScopeNotFound = "scope cannot be found";

            public const string PolicyNotFound = "authorization policy cannot be found";
        }

        public static class PostPermissionNames
        {
            public const string ResourceSetId = "resource_set_id";

            public const string Scopes = "scopes";
        }

        public static class AddPermissionResponseNames
        {
            public const string TicketId = "ticket_id";
        }

        public static class CachingStoreNames
        {
            public const string GetResourceStoreName = "GetResource_";
            public const string GetResourcesStoreName = "GetResources";
            public const string GetPolicyStoreName = "GetPolicy_";
            public const string GetPoliciesStoreName = "GetPolicies";
        }
    }
}
