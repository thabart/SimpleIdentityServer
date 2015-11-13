namespace SimpleIdentityServer.Core.Errors
{
    public static class ErrorDescriptions
    {
        public static string MissingParameter = "the parameter {0} is missing";

        public static string RequestIsNotValid =  "the request is not valid";

        public static string ClientIsNotValid = "the client_id {0} is not valid";

        public static string RedirectUrlIsNotValid = "the redirect url {0} is not valid";

        public static string ResourceOwnerCredentialsAreNotValid = "resource owner credentials are not valid";

        public static string ParameterIsNotCorrect = "the paramater {0} is not correct";

        public static string ScopesAreNotAllowedOrInvalid = "the scopes {0} are not allowed or invalid";

        public static string DuplicateScopeValues = "the scopes {0} are duplicated";

        public static string TheScopesNeedToBeSpecified = "the scopes {0} need to be specified";

        public static string TheUserNeedsToBeAuthenticated = "the user needs to be authenticated";

        public static string TheUserNeedsToGiveIsConsent = "the user needs to give his consent";

        public static string TheUserCannotBeReauthenticated = "The user cannot be reauthenticated";

        public static string TheRedirectionUriIsNotWellFormed = "Based on the RFC-3986 the redirection-uri is not well formed";

        public static string AtLeastOneResponseTypeIsNotSupported =
            "at least one response_type parameter is not supported";

        public static string AtLeastOnePromptIsNotSupported =
            "at least one prompt parameter is not supported";

        public static string PromptParameterShouldHaveOnlyNoneValue = "prompt parameter should have only none value";

        public static string TheAuthorizationFlowIsNotSupported = "the authorization flow is not supported";

        public static string TheClientDoesntSupportTheResponseType = "the client {0} doesn't support the response type {1}";

        public static string TheClientDoesntSupportTheGrantType = "the client {0} doesn't support the grant type {1}";

        public static string TheIdTokenCannotBeSigned = "the id token cannot be signed";

        public static string TheClientCannotBeAuthenticated = "the client cannot be authenticated";

        public static string TheAuthorizationCodeIsNotCorrect = "the authorization code is not correct";

        public static string TheAuthorizationCodeHasNotBeenIssuedForTheGivenClientId =
            "the authorization code has not been issued for the given client id {0}";

        public static string TheAuthorizationCodeIsObsolete = "the authorization code is obsolete";

        public static string TheRedirectionUrlIsNotTheSame =
            "the redirection url is not the same than the one passed in the authorization request";
    }
}
