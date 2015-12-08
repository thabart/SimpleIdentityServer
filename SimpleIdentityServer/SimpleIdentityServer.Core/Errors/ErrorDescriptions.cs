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

        public static string TheClaimCannotBeFetch = "the claims cannot be fetch";

        public static string TheUserNeedsToGiveHisConsent = "the user needs to give his consent";

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

        public static string TheClientAssertionIsNotAJwtToken = "the client assertion is not a JWT token";

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

        public static string TheSignatureOfIdTokenHintParameterCannotBeChecked = "the signature of id token hint parameter cannot be checked";

        public static string TheIdentityTokenDoesntContainSimpleIdentityServerAsAudience = "the identity token doesnt contain simple identity server in the audience";

        public static string TheCurrentAuthenticatedUserDoesntMatchWithTheIdentityToken = "the current authenticated user doesn't match with the identity token";
    }
}
