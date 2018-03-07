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
            public const string Configuration = ".well-known/uma2-configuration";
            public const string ResourceSet = "rs/resource_set";
            public const string Permission = "/perm";
            public const string Policies = "/policies";
            public const string Introspection = "/introspect";
            public const string Token = "/token";
            public const string Jwks = "/jwks";
            public const string Registration = "/registration";
            public const string DiscoveryAction = ".well-known/openid-configuration";
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

        public static class CachingStoreNames
        {
            public const string GetResourceStoreName = "GetResource_";
            public const string GetResourcesStoreName = "GetResources";
            public const string GetPolicyStoreName = "GetPolicy_";
            public const string GetPoliciesStoreName = "GetPolicies";
        }
    }
}
