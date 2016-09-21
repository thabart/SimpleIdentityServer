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

namespace SimpleIdentityServer.Core.Errors
{
    public static class ErrorDescriptions
    {
        public const string MissingParameter = "the parameter {0} is missing";
        public const string RequestIsNotValid =  "the request is not valid";
        public const string ClientIsNotValid = "the client id parameter {0} doesn't exist or is not valid";
        public const string RedirectUrlIsNotValid = "the redirect url {0} doesn't exist or is not valid";
        public const string ResourceOwnerCredentialsAreNotValid = "resource owner credentials are not valid";
        public const string ParameterIsNotCorrect = "the paramater {0} is not correct";
        public const string ScopesAreNotAllowedOrInvalid = "the scopes {0} are not allowed or invalid";
        public const string DuplicateScopeValues = "duplicate scopes {0} have been passed in parameter";
        public const string TheScopesNeedToBeSpecified = "the scope(s) {0} need(s) to be specified";
        public const string TheUserNeedsToBeAuthenticated = "the user needs to be authenticated";
        public const string TheAuthorizationRequestCannotBeProcessedBecauseThereIsNotValidPrompt =
            "the authorization request cannot be processed because there is not valid prompt";
        public const string TheClaimCannotBeFetch = "the claims cannot be fetch";
        public const string TheUserNeedsToGiveHisConsent = "the user needs to give his consent";
        public const string ThePromptParameterIsNotSupported = "the prompt parameter {0} is not supported";
        public const string TheUserCannotBeReauthenticated = "The user cannot be reauthenticated";
        public const string TheRedirectionUriIsNotWellFormed = "Based on the RFC-3986 the redirection-uri is not well formed";
        public const string AtLeastOneResponseTypeIsNotSupported =
            "at least one response_type parameter is not supported";
        public const string AtLeastOnePromptIsNotSupported =
            "at least one prompt parameter is not supported";
        public const string PromptParameterShouldHaveOnlyNoneValue = "prompt parameter should have only none value";
        public const string TheAuthorizationFlowIsNotSupported = "the authorization flow is not supported";
        public const string TheClientDoesntSupportTheResponseType = "the client '{0}' doesn't support the response type: '{1}'";
        public const string TheClientDoesntSupportTheGrantType = "the client {0} doesn't support the grant type {1}";
        public const string TheIdTokenCannotBeSigned = "the id token cannot be signed";
        public const string TheClientCannotBeAuthenticated = "the client cannot be authenticated";
        public const string TheClientCannotBeAuthenticatedWithSecretBasic = "the client cannot be authenticated with secret basic";
        public const string TheClientCannotBeAuthenticatedWithSecretPost = "the client cannot be authenticated with secret post";
        public const string TheAuthorizationCodeIsNotCorrect = "the authorization code is not correct";
        public const string TheAuthorizationCodeHasNotBeenIssuedForTheGivenClientId =
            "the authorization code has not been issued for the given client id {0}";
        public const string TheTokenHasNotBeenIssuedForTheGivenClientId = "the token has not been issued for the given client id '{0}'";
        public const string TheAuthorizationCodeIsObsolete = "the authorization code is obsolete";
        public const string TheRedirectionUrlIsNotTheSame =
            "the redirection url is not the same than the one passed in the authorization request";
        public const string TheIdServerIsNotPresentInTheAudience = "the identity server is not present in the audience";
        public const string TheJwtTokenHasAlreadyBeenUsed = "the jwt token has already been used";
        public const string TheJwtTokenIsExpired = "the jwt token is expired";
        public const string TheTokenIsExpired = "the token is expired";
        public const string TheTokenIsNotValid = "the token is not valid";
        public const string TheIssuerFromJwtIsNotCorrect = "the issuer from JWT is not correct";
        public const string TheClientIdPassedInJwtIsNotCorrect = "the client id passed in JWT is not correct";
        public const string TheAudiencePassedInJwtIsNotCorrect = "the audience passed in JWT is not correct";
        public const string TheReceivedJwtHasExpired = "the received JWT has expired";
        public const string TheSignatureIsNotCorrect = "the signature is not correct";
        public const string TheHeaderCannotBeExtractedFromJwsToken = "the header cannot be extracted from JWS token";
        public const string TheJwsPayloadCannotBeExtracted = "the jws payload cannot be extracted";
        public const string TheHeaderCannotBeExtractedFromJweToken = "the header cannot be extracted from JWE token";
        public const string TheJweTokenCannotBeDecrypted = "the jwe token cannot be decrypted";
        public const string TheClientAssertionIsNotAJwsToken = "the client assertion is not a JWS token";
        public const string TheClientAssertionIsNotAJweToken = "the client assertion is not a JWE token";
        public const string TheJwsPayLoadCannotBeExtractedFromTheClientAssertion =
            "the jws payload cannot be extracted from the client assertion";
        public const string TheClientAssertionCannotBeDecrypted =
            "the client assertion cannot be decrypted";
        public const string TheClaimIsNotValid = "the claim {0} is not valid";
        public const string TheRequestUriParameterIsNotWellFormed = "the request_uri parameter is not well formed";
        public const string TheRequestDownloadedFromRequestUriIsNotValid =
            "the request downloaded from request URI is not valid";
        public const string TheRequestParameterIsNotCorrect = "the request parameter is not correct";
        public const string TheIdTokenHintParameterCannotBeDecrypted = "the id token hint parameter cannot be decrypted";
        public const string TheIdTokenHintParameterIsNotAValidToken = "the id_token parameter is not a valid token";
        public const string TheSignatureOfIdTokenHintParameterCannotBeChecked = "the signature of id token hint parameter cannot be checked";
        public const string TheIdentityTokenDoesntContainSimpleIdentityServerAsAudience = "the identity token doesnt contain simple identity server in the audience";
        public const string TheCurrentAuthenticatedUserDoesntMatchWithTheIdentityToken = "the current authenticated user doesn't match with the identity token";
        public const string TheResponseCannotBeGeneratedBecauseResourceOwnerNeedsToBeAuthenticated =
            "the response cannot be generated because the resource owner needs to be authenticated";
        public const string TheRedirectUriParameterIsNotValid = "one or more redirect_uri values are invalid";
        public const string TheRedirectUriContainsAFragment = "one or more redirect_uri contains a fragment";
        public const string TheJwksParameterCannotBeSetBecauseJwksUrlIsUsed =
            "the jwks parameter cannot be set because the Jwks Url has already been set";
        public const string TheParameterIsTokenEncryptedResponseAlgMustBeSpecified =
            "the parameter id_token_encrypted_response_alg must be specified";
        public const string TheParameterUserInfoEncryptedResponseAlgMustBeSpecified =
            "the parameter userinfo_encrypted_response_alg must be specified";
        public const string TheParameterRequestObjectEncryptionAlgMustBeSpecified =
            "the parameter request_object_encryption_alg must be specified";
        public const string OneOfTheRequestUriIsNotValid = "one of the request_uri is not valid";
        public const string TheSectorIdentifierUrisCannotBeRetrieved = "the sector identifier uris cannot be retrieved";
        public const string OneOrMoreSectorIdentifierUriIsNotARedirectUri =
            "one or more sector uri is not a redirect_uri";
        public const string TheRefreshTokenIsNotValid = "the refresh token is not valid";
        public const string TheRequestCannotBeExtractedFromTheCookie =
            "the request cannot be extracted from the cookie";
        public const string AnErrorHasBeenRaisedWhenTryingToAuthenticate =
            "an error {0} has been raised when trying to authenticate";
        public const string TheLoginInformationCannotBeExtracted = "the login information cannot be extracted";
        public const string TheResourceOwnerCredentialsAreNotCorrect = "the resource owner credentials are not correct";
        public const string TheExternalProviderIsNotSupported = "the external provider {0} is not supported";
        public const string NoSubjectCanBeExtracted = "no subject can be extracted";
        public const string TheTokenDoesntExist = "the token doesn't exist";
        public const string TheRoCannotBeCreated = "the resource owner cannot be created because subject is missing";
        public const string TheSubjectCannotBeRetrieved = "the subject cannot be retrieved";
        public const string TheRoDoesntExist = "the resource owner doesn't exist";
        public const string TheRoWithCredentialsAlreadyExists = "a resource owner with same credentials already exists";
        public const string TheAccountHasAlreadyBeenActivated = "the accout has already been activated";
        public const string TwoFactorAuthenticationCannotBePerformed = "two factor authentication cannot be performed";
        public const string TwoFactorAuthenticationIsNotEnabled = "two factor authentication is not enabled";
        public const string TheConfirmationCodeCannotBeSaved = "the confirmation code cannot be saved";
    }
}
