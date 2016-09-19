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

namespace SimpleIdentityServer.Scim.Startup
{
    public class Constants
    {
        public static class MultiValueAttributeNames
        {
            public const string Type = "type";
            public const string Display = "display";
            public const string Primary = "primary";
            public const string Value = "value";
            public const string Ref = "$ref";
        }

        public static class ScimResourceNames
        {
            public const string Schemas = "schemas";
            public const string Meta = "meta";
        }

        public static class IdentifiedScimResourceNames
        {
            public const string Id = "id";
            public const string ExternalId = "externalId";
        }

        public static class MetaResponseNames
        {
            public const string ResourceType = "resourceType";
            public const string Created = "created";
            public const string LastModified = "lastModified";
            public const string Location = "location";
            public const string Version = "version";
        }

        public static class ServiceProviderConfigResponseNames
        {
            public const string DocumentationUri = "documentationUri";
            public const string Patch = "patch";
            public const string Bulk = "bulk";
            public const string Filter = "filter";
            public const string ChangePassword = "changePassword";
            public const string Sort = "sort";
            public const string Etag = "etag";
            public const string AuthenticationSchemes = "authenticationSchemes";
        }

        public static class PatchResponseNames
        {
            public const string Supported = "supported";
        }

        public static class BulkResponseNames
        {
            public const string Supported = "supported";
            public const string MaxOperations = "maxOperations";
            public const string MaxPayloadSize = "maxPayloadSize";
        }

        public static class FilterResponseNames
        {
            public const string Supported = "supported";
            public const string MaxResults = "maxResults";
        }

        public static class ChangePasswordResponseNames
        {
            public const string Supported = "supported";
        }

        public static class SortResponseNames
        {
            public const string Supported = "supported";
        }

        public static class EtagResponseNames
        {
            public const string Supported = "supported";
        }

        public static class AuthenticationSchemeResponseNames
        {
            public const string Type = "type";
            public const string Name = "name";
            public const string Description = "description";
            public const string SpecUri = "specUri";
            public const string DocumentationUri = "documentationUri";
        }
    }
}
