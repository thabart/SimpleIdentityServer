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

namespace SimpleIdentityServer.Scim.Core.Errors
{
    public static class ErrorMessages
    {
        public const string TheSchemaDoesntExist = "the schema {0} doesn't exist";
        public const string TheAttributeIsRequired = "the attribute {0} is required";
        public const string TheAttributeTypeIsNotCorrect = "the attribute {0} is not a {1} or {1}[]";
        public const string TheAttributeTypeIsNotSupported = "the attribute type {0} is not supported";
        public const string TheAttributeIsNotAnArray = "the attribute {0} is not an array";
        public const string TheAttributeIsNotComplex = "the attribute {0} is not complex";
        public const string TheRequestCannotBeParsedForSomeReason = "the request cannot be parsed for some reason";
        public const string TheComplexAttributeArrayShouldContainsOnlyComplexAttribute = "complex attribute array should contains only complex attribute";
        public const string TheResourceDoesntExist = "Resource {0} not found";
        public const string TheRepresentationCannotBeRemoved = "something goes wrong when trying to remove the resource";
    }
}
