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

namespace SimpleIdentityServer.Manager.Core.Errors
{
    public static class ErrorDescriptions
    {
        public const string TheUrlIsNotWellFormed = "the url {0} is not well formed";
        public const string TheTokenIsNotAValidJws = "the token is not a valid JWS";
        public const string TheTokenIsNotAValidJwe = "the token is not a valid JWE";
        public const string TheJsonWebKeyCannotBeFound = "the json web key {0} cannot be found {1}";
        public const string TheSignatureIsNotCorrect = "the signature is not correct";
        public const string TheSignatureCannotBeChecked = "the signature cannot be checked if the URI is not specified";
        public const string TheJwsCannotBeGeneratedBecauseMissingParameters = "the jws cannot be generated because either the Url or Kid is not specified";
        public const string TheKtyIsNotSupported = "the kty '{0}' is not supported";
        public const string TheContentCannotBeExtractedFromJweToken = "the content cannot be extracted from the jwe token";
        public const string TheClientDoesntExist = "the client '{0}' doesn't exist";
        public static string MissingParameter = "the parameter {0} is missing";
        public const string TheScopeDoesntExist = "the scope '{0}' doesn't exist";
        public static string TheRedirectUriParameterIsNotValid = "one or more redirect_uri values are invalid";
        public static string TheRedirectUriContainsAFragment = "one or more redirect_uri contains a fragment";
        public static string ParameterIsNotCorrect = "the paramater {0} is not correct";
        public static string TheJwksParameterCannotBeSetBecauseJwksUrlIsUsed =
            "the jwks parameter cannot be set because the Jwks Url has already been set";
        public static string OneOrMoreSectorIdentifierUriIsNotARedirectUri =
            "one or more sector uri is not a redirect_uri";
        public static string TheParameterIsTokenEncryptedResponseAlgMustBeSpecified =
            "the parameter id_token_encrypted_response_alg must be specified";
        public static string OneOfTheRequestUriIsNotValid = "one of the request_uri is not valid";
        public static string TheParameterRequestObjectEncryptionAlgMustBeSpecified =
            "the parameter request_object_encryption_alg must be specified";                
        public static string TheParameterUserInfoEncryptedResponseAlgMustBeSpecified =
            "the parameter userinfo_encrypted_response_alg must be specified";
        public static string TheSectorIdentifierUrisCannotBeRetrieved = "the sector identifier uris cannot be retrieved";
        public static string TheResourceOwnerDoesntExist = "the resource owner {0} doesn't exist";
        public static string TheResourceOwnerMustBeConfirmed = "the account must be confirmed";
        public static string TheScopeAlreadyExists = "The scope {0} already exists";
    }
}
