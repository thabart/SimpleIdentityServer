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

namespace SimpleIdentityServer.Uma.Core.Errors
{
    internal static class ErrorDescriptions
    {
        public const string TheParameterNeedsToBeSpecified = "the parameter {0} needs to be specified";           
        public const string TheUrlIsNotWellFormed = "the url {0} is not well formed";
        public const string TheResourceSetCannotBeInserted = "an error occured while trying to insert the resource set";
        public const string TheResourceSetDoesntExist = "resource set {0} doesn't exist";
        public const string SomeResourcesDontExist = "some resources don't exist";
        public const string AtLeastOneResourceDoesntExist = "at least one resource doesn't exist";
        public const string ThePolicyDoesntContainResource = "the authorization policy doesn't contain the resource";
        public const string TheResourceSetCannotBeUpdated = "resource set {0} cannot be udpated";
        public const string TheResourceSetCannotBeRemoved = "resource set {0} cannot be removed";
        public const string TheResourceSetCannotBeRetrieved = "resource set {0} cannot be retrieved";
        public const string TheResourceSetsCannotBeRetrieved = "resource sets cannot be retrieved";
        public const string TheScopeCannotBeRetrieved = "scope cannot be retrieved";
        public const string TheScopeCannotBeInserted = "scope cannot be inserted";
        public const string TheScopeCannotBeUpdated = "scope cannot be updated";
        public const string TheScopeCannotBeRemoved = "scope cannot be removed";
        public const string TheScopesCannotBeRetrieved = "scopes cannot be retrieved";
        public const string TheScopeAlreadyExists = "scope {0} already exists";
        public const string TheScopeAreNotValid = "one or more scopes are not valid";
        public const string TheSchemeIsNotCorrect = "authorization scheme is not correct";
        public const string AtLeastOneTicketCannotBeInserted = "at least one ticket cannot be inserted";
        public const string AtLeastOneTicketDoesntExist = "at least one ticket doesn't exist";
        public const string TheTicketIssuerIsDifferentFromTheClient = "the ticket issuer is different from the client";
        public const string TheTicketIsExpired = "the ticket is expired";
        public const string TheTicketDoesntExist = "the ticket {0} doesn't exist";
        public const string TheTicketAlreadyExists = "the ticket  already exists";
        public const string TheRptCannotBeInserted = "the rpt cannot be inserted";
        public const string ThePolicyCannotBeInserted = "the authorization policy cannot be inserted";
        public const string ThePolicyCannotBeUpdated = "the authorization policy cannot be updated";
        public const string OneOrMoreScopesDontBelongToAResourceSet = "one or more scopes don't belong to a resource set";
        public const string TheAuthorizationPolicyCannotBeRetrieved = "the authorization policy {0} cannot be retrieved";
        public const string TheAuthorizationPolicyCannotBeUpdated = "the authorization policy {0} cannot be updated";
        public const string TheAutorizationPoliciesCannotBeRetrieved = "the authorization policies cannot be retrieved";
        public const string TheRptsDontExist = "the rpts {0} don't exist";
        public const string TheRptIsExpired = "the rpt is expired";
        public const string TheAuthorizationPolicyDoesntExist = "the authorization policy {0} doesn't exist";
        public const string TheClaimTokenIsNotValid = "the claim token parameter is not valid";
        public const string TheAuthorizationPolicyIsNotSatisfied = "the authorization policy is not satisfied";
        public const string TheClientDoesntSupportTheGrantType = "the client doesn't support the grant type {0}";
    }
}