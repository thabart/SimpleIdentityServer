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
        public static string MissingParameter = "the parameter {0} is missing";

        public static string RequestIsNotValid =  "the request is not valid";

        public static string ClientIsNotValid = "the client id parameter {0} doesn't exist or is not valid";

        public static string RedirectUrlIsNotValid = "the redirect url {0} doesn't exist or is not valid";

        public static string ResourceOwnerCredentialsAreNotValid = "resource owner credentials are not valid";

        public static string ParameterIsNotCorrect = "the paramater {0} is not correct";

        public static string ScopesAreNotAllowedOrInvalid = "the scopes {0} are not allowed or invalid";

        public static string DuplicateScopeValues = "duplicate scopes {0} have been passed in parameter";

        public static string TheScopesNeedToBeSpecified = "the scope(s) {0} need(s) to be specified";

        public static string TheUserNeedsToBeAuthenticated = "the user needs to be authenticated";

        public static string TheAuthorizationRequestCannotBeProcessedBecauseThereIsNotValidPrompt =
            "the authorization request cannot be processed because there is not valid prompt";

        public static string TheClaimCannotBeFetch = "the claims cannot be fetch";

        public static string TheUserNeedsToGiveHisConsent = "the user needs to give his consent";

        public static string ThePromptParameterIsNotSupported = "the prompt parameter {0} is not supported";

        public static string TheUserCannotBeReauthenticated = "The user cannot be reauthenticated";

        public static string TheRedirectionUriIsNotWellFormed = "Based on the RFC-3986 the redirection-uri is not well formed";

        public static string AtLeastOneResponseTypeIsNotSupported =
            "at least one response_type parameter is not supported";

        public static string AtLeastOnePromptIsNotSupported =
            "at least one prompt parameter is not supported";

        public static string PromptParameterShouldHaveOnlyNoneValue = "prompt parameter should have only none value";

        public static string TheAuthorizationFlowIsNotSupported = "the authorization flow is not supported";

        public static string TheClientDoesntSupportTheResponseType = "the client '{0}' doesn't support the response type: '{1}'";

        public static string TheClientDoesntSupportTheGrantType = "the client {0} doesn't support the grant type {1}";

        public static string TheIdTokenCannotBeSigned = "the id token cannot be signed";

        public static string TheClientCannotBeAuthenticated = "the client cannot be authenticated";

        public static string TheAuthorizationCodeIsNotCorrect = "the authorization code is not correct";

        public static string TheAuthorizationCodeHasNotBeenIssuedForTheGivenClientId =
            "the authorization code has not been issued for the given client id {0}";

        public static string TheAuthorizationCodeIsObsolete = "the authorization code is obsolete";

        public static string TheRedirectionUrlIsNotTheSame =
            "the redirection url is not the same than the one passed in the authorization request";

        public static string TheIdServerIsNotPresentInTheAudience = "the identity server is not present in the audience";

        public static string TheJwtTokenHasAlreadyBeenUsed = "the jwt token has already been used";

        public static string TheJwtTokenIsExpired = "the jwt token is expired";

        public static string TheAccessTokenIsExpired = "the access token is expired";

        public static string TheAccessTokenIsNotValid = "the access token is not valid";

        public static string TheIssuerFromJwtIsNotCorrect = "the issuer from JWT is not correct";

        public static string TheClientIdPassedInJwtIsNotCorrect = "the client id passed in JWT is not correct";

        public static string TheAudiencePassedInJwtIsNotCorrect = "the audience passed in JWT is not correct";

        public static string TheReceivedJwtHasExpired = "the received JWT has expired";

        public static string TheSignatureIsNotCorrect = "the signature is not correct";

        public static string TheHeaderCannotBeExtractedFromJwsToken = "the header cannot be extracted from JWS token";

        public static string TheJwsPayloadCannotBeExtracted = "the jws payload cannot be extracted";

        public static string TheHeaderCannotBeExtractedFromJweToken = "the header cannot be extracted from JWE token";

        public static string TheJweTokenCannotBeDecrypted = "the jwe token cannot be decrypted";

        public static string TheClientAssertionIsNotAJwsToken = "the client assertion is not a JWS token";

        public static string TheClientAssertionIsNotAJweToken = "the client assertion is not a JWE token";

        public static string TheJwsPayLoadCannotBeExtractedFromTheClientAssertion =
            "the jws payload cannot be extracted from the client assertion";

        public static string TheClientAssertionCannotBeDecrypted =
            "the client assertion cannot be decrypted";

        public static string TheClaimIsNotValid = "the claim {0} is not valid";

        public static string TheRequestUriParameterIsNotWellFormed = "the request_uri parameter is not well formed";

        public static string TheRequestDownloadedFromRequestUriIsNotValid =
            "the request downloaded from request URI is not valid";

        public static string TheRequestParameterIsNotCorrect = "the request parameter is not correct";

        public static string TheIdTokenHintParameterCannotBeDecrypted = "the id token hint parameter cannot be decrypted";

        public static string TheIdTokenHintParameterIsNotAValidToken = "the id_token parameter is not a valid token";

        public static string TheSignatureOfIdTokenHintParameterCannotBeChecked = "the signature of id token hint parameter cannot be checked";

        public static string TheIdentityTokenDoesntContainSimpleIdentityServerAsAudience = "the identity token doesnt contain simple identity server in the audience";

        public static string TheCurrentAuthenticatedUserDoesntMatchWithTheIdentityToken = "the current authenticated user doesn't match with the identity token";

        public static string TheResponseCannotBeGeneratedBecauseResourceOwnerNeedsToBeAuthenticated =
            "the response cannot be generated because the resource owner needs to be authenticated";

        public static string TheRedirectUriParameterIsNotValid = "one or more redirect_uri values are invalid";

        public static string TheRedirectUriContainsAFragment = "one or more redirect_uri contains a fragment";

        public static string TheJwksParameterCannotBeSetBecauseJwksUrlIsUsed =
            "the jwks parameter cannot be set because the Jwks Url has already been set";

        public static string TheParameterIsTokenEncryptedResponseAlgMustBeSpecified =
            "the parameter id_token_encrypted_response_alg must be specified";

        public static string TheParameterUserInfoEncryptedResponseAlgMustBeSpecified =
            "the parameter userinfo_encrypted_response_alg must be specified";

        public static string TheParameterRequestObjectEncryptionAlgMustBeSpecified =
            "the parameter request_object_encryption_alg must be specified";

        public static string OneOfTheRequestUriIsNotValid = "one of the request_uri is not valid";

        public static string TheSectorIdentifierUrisCannotBeRetrieved = "the sector identifier uris cannot be retrieved";

        public static string OneOrMoreSectorIdentifierUriIsNotARedirectUri =
            "one or more sector uri is not a redirect_uri";

        public static string TheRefreshTokenIsNotValid = "the refresh token is not valid";

        public static string TheRequestCannotBeExtractedFromTheCookie =
            "the request cannot be extracted from the cookie";

        public static string AnErrorHasBeenRaisedWhenTryingToAuthenticate =
            "an error {0} has been raised when trying to authenticate";

        public static string TheLoginInformationCannotBeExtracted = "the login information cannot be extracted";

        public static string TheResourceOwnerCredentialsAreNotCorrect = "the resource owner credentials are not correct";

        public static string TheExternalProviderIsNotSupported = "the external provider {0} is not supported";

        public static string NoSubjectCanBeExtracted = "no subject can be extracted";
    }
}
