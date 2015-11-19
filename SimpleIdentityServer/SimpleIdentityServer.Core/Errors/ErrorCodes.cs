namespace SimpleIdentityServer.Core.Errors
{
    public static class ErrorCodes
    {
        public static string InvalidRequestCode = "invalid_request";

        public static string InvalidClient = "invalid_client";

        public static string InvalidGrant = "invalid_grant";

        public static string InvalidToken = "invalid_token";

        public static string UnAuthorizedClient = "unauthorized_client";

        public static string UnSupportedGrantType = "unsupported_grant_type";

        public static string InvalidScope = "invalid_scope";

        public static string InvalidRequestUriCode = "invalid_request_uri";

        public static string LoginRequiredCode = "login_required";

        public static string InteractionRequiredCode = "interaction_required";
    }
}
