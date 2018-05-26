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

namespace SimpleIdentityServer.Scim.Common
{
    public static class Constants
    {
        public static class MultiValueAttributeNames
        {
            public const string Type = "type";
            public const string Display = "display";
            public const string Primary = "primary";
            public const string Value = "value";
            public const string Ref = "$ref";
        }

        public static class ScimTypeValues
        {
            public const string Mutability = "mutability";
            public const string InvalidFilter = "invalidFilter";
            public const string TooMany = "tooMany";
            public const string Uniqueness = "uniqueness";
            public const string InvalidSyntax = "invalidSyntax";
            public const string InvalidPath = "invalidPath";
            public const string NoTarget = "noTarget";
            public const string InvalidValue = "invalidValue";
            public const string InvalidVers = "invalidVers";
            public const string Sensitive = "sensitive";
        }

        public static class UserResourceResponseNames
        {
            public const string UserName = "userName";
            public const string Name = "name";
            public const string DisplayName = "displayName";
            public const string NickName = "nickName";
            public const string ProfileUrl = "profileUrl";
            public const string Title = "title";
            public const string UserType = "userType";
            public const string PreferredLanguage = "preferredLanguage";
            public const string Locale = "locale";
            public const string Timezone = "timezone";
            public const string Active = "active";
            public const string Password = "password";
            public const string Emails = "emails";
            public const string Phones = "phones";
            public const string Ims = "ims";
            public const string Photos = "photos";
            public const string Addresses = "addresses";
            public const string Groups = "groups";
            public const string Entitlements = "entitlements";
            public const string Roles = "roles";
            public const string X509Certificates = "x509Certificates";
        }

        public static class GroupResourceResponseNames
        {
            public const string DisplayName = "displayName";
            public const string Members = "members";
        }

        public static class AddressResponseNames
        {
            public const string Formatted = "formatted";
            public const string StreetAddress = "streetAddress";
            public const string Locality = "locality";
            public const string Region = "region";
            public const string PostalCode = "postalCode";
            public const string Country = "country";
        }

        public static class GroupMembersResponseNames
        {
            public const string Value = "value";
        }

        public static class NameResponseNames
        {
            public const string Formatted = "formatted";
            public const string FamilyName = "familyName";
            public const string GivenName = "givenName";
            public const string MiddleName = "middleName";
            public const string HonorificPrefix = "honorificPrefix";
            public const string HonorificSuffix = "honorificSuffix";
        }

        public static class ScimResourceNames
        {
            public const string Schemas = "schemas";
            public const string Meta = "meta";
        }

        public static class SearchParameterNames
        {
            public const string Attributes = "attributes";
            public const string ExcludedAttributes = "excludedAttributes";
            public const string Filter = "filter";
            public const string SortBy = "sortBy";
            public const string SortOrder = "sortOrder";
            public const string StartIndex = "startIndex";
            public const string Count = "count";
        }

        public static class BulkRequestNames
        {
            public const string FailOnErrors = "failOnErrors";
            public const string Operations = "Operations";
        }

        public static class BulkOperationRequestNames
        {
            public const string Method = "method";
            public const string BulkId = "bulkId";
            public const string Version = "version";
            public const string Path = "path";
            public const string Data = "data";
        }

        public static class BulkOperationResponseNames
        {
            public const string Location = "location";
            public const string Response = "response";
            public const string Status = "status";
        }

        public static class SearchParameterResponseNames
        {
            public static string TotalResults = "totalResults";
            public static string Resources = "Resources";
            public static string StartIndex = "startIndex";
            public static string ItemsPerPage = "itemsPerPage";
        }

        public static class PatchOperationsRequestNames
        {
            public const string Operations = "Operations";
        }

        public static class PatchOperationRequestNames
        {
            public const string Operation = "op";
            public const string Path = "path";
            public const string Value = "value";
        }

        public static class ErrorResponseNames
        {
            public const string Detail = "detail";
            public const string Status = "status";
        }

        public static class EnrichedErrorResponseNames
        {
            public const string ScimType = "scimType";
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

        public static class SchemaResponseNames
        {
            public const string Id = "id";
            public const string Name = "name";
            public const string Description = "description";
            public const string Attributes = "attributes";
        }

        public static class SchemaAttributeResponseNames
        {
            public const string Name = "name";
            public const string Type = "type";
            public const string MultiValued = "multiValued";
            public const string Description = "description";
            public const string Required = "required";
            public const string CanonicalValues = "canonicalValues";
            public const string CaseExact = "caseExact";
            public const string Mutability = "mutability";
            public const string Returned = "returned";
            public const string Uniqueness = "uniqueness";
            public const string ReferenceTypes = "referenceTypes";
        }

        public static class ComplexSchemaAttributeResponseNames
        {
            public const string SubAttributes = "subAttributes";
        }
        
        public static class SchemaAttributeTypes
        {
            public const string String = "string";
            public const string Boolean = "boolean";
            public const string Decimal = "decimal";
            public const string DateTime = "dateTime";
            public const string Integer = "integer";
            public const string Binary = "binary";
            public const string Complex = "complex";
            public const string Reference = "reference";
        }

        public static class SchemaAttributeMutability
        {
            /// <summary>
            /// Attribute shall not be modified
            /// </summary>
            public const string ReadOnly = "readOnly";
            /// <summary>
            /// May be updated and read at any time
            /// </summary>
            public const string ReadWrite = "readWrite";
            /// <summary>
            /// Attribute may be defined at resource creation or at record replacement
            /// </summary>
            public const string Immutable = "immutable";
            /// <summary>
            /// The attribute may be updated at any time
            /// </summary>
            public const string writeOnly = "writeOnly";
        }

        public static class SchemaAttributeReturned
        {
            /// <summary>
            /// Always returned
            /// </summary>
            public const string Always = "always";
            /// <summary>
            /// Never returned
            /// </summary>
            public const string Never = "never";
            /// <summary>
            /// Returned by default in all SCIM oprations
            /// </summary>
            public const string Default = "default";
            /// <summary>
            /// Returned in response to any PUT, POST or PATCH operations
            /// </summary>
            public const string Request = "request";
        }

        public static class SchemaAttributeUniqueness
        {
            /// <summary>
            /// Values are not intended to be unique
            /// </summary>
            public const string None = "none";
            /// <summary>
            /// Value should be unique within the context of the current SCIM endpoint
            /// </summary>
            public const string Server = "server";
            /// <summary>
            /// Value should be globally unique
            /// </summary>
            public const string Global = "global";
        }

        public static class SchemaUrns
        {
            public const string Group = "urn:ietf:params:scim:schemas:core:2.0:Group";
            public const string User = "urn:ietf:params:scim:schemas:core:2.0:User";
            public const string ServiceProvider = "urn:ietf:params:scim:schemas:core:2.0:ServiceProviderConfig";
        }

        public static class Messages
        {
            public const string Error = "urn:ietf:params:scim:api:messages:2.0:Error";
            public const string PatchOp = "urn:ietf:params:scim:api:messages:2.0:PatchOp";
            public const string ListResponse = "urn:ietf:params:scim:api:messages:2.0:ListResponse";
            public const string Bulk = "urn:ietf:params:scim:api:messages:2.0:BulkRequest";
            public const string BulkResponse = "urn:ietf:params:scim:api:messages:2.0:BulkResponse";
        }

        public static class ResourceTypes
        {
            public const string Group = "Group";
            public const string User = "User";
        }

        public static class SortOrderNames
        {
            public const string Ascending = "ascending";
            public const string Descending = "descending";
        }
    }
}
