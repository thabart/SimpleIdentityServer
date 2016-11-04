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
        public const string TheRepresentationCannotBeAdded = "something goes wrong when trying to added the resource";
        public const string TheRepresentationCannotBeSet = "something goes wrong when trying to replace the resource";
        public const string TheLocationPatternIsNotCorrect = "the location pattern {0} is not correct";
        public const string TheImmutableAttributeCannotBeUpdated = "the immutable attribute {0} cannot be updated";
        public const string TheRepresentationCannotBeUpdated = "the representation cannot be updated";
        public const string TheRequestIsNotAPatchOperation = "the request is not a patch operation because the schema is not correct";
        public const string TheRequestIsNotABulkOperation = "the request is not a bulk operation because the schema is not correct";
        public const string ThePatchOperationIsNotSupported = "the patch operation '{0}' is not supported";
        public const string TheBulkMethodIsNotSupported = "the bulk method '{0}' is not supported";
        public const string TheBulkOperationPathIsRequired = "the bulk operation path is required";
        public const string TheBulkOperationPathIsNotSupported = "the bulk operation path '{0}' is not supported";
        public const string TheBulkDataParameterMustBeSpecified = "the 'data' parameter must be specified";
        public const string TheBulkIdParameterMustBeSpecified = "the 'bulkId' parameter must be specified";
        public const string ThePathNeedsToBeSpecified = "the path needs to be specified";
        public const string TheValueNeedsToBeSpecified = "the value needs to be specified";
        public const string TheValueIsNotCompliantWithTheSchema = "the value is not compliant with the schema {0}";
        public const string TheRepresentationCannotBeRemovedBecauseItsNotAnArray = "cannot be removed because it's not an array";
        public const string TheAttributeDoesntExist = "the attribute {0} doesn't exist";
        public const string TheRepresentationCannotBeAddedBecauseItsNotAnArray = "cannot be added because it's not an array";
        public const string TheFilterIsNotCorrect = "the filter is not correct";
        public const string TheOperationsParameterMustBeSpecified = "the operations parameter must be specified";
        public const string TheValueMustBeSpecified = "the value parameter must be specified";
        public const string TheParameterIsNotValid = "the parameter {0} is not valid";
        public const string TheMaximumNumberOfErrorHasBeenReached = "the maximum number of errors '{0}' has been reached";
    }
}
