namespace SimpleIdentityServer.Core.Errors
{
    public static class ErrorCodes
    {
        public static string InvalidRequestCode = "invalid_request";

        public static string InvalidClient = "invalid_client";

        public static string InvalidGrant = "invalid_grant";

        public static string UnAuthorizedClient = "unauthorized_client";

        public static string UnSupportedGrantType = "unsupported_grant_type";

        public static string InvalidScope = "invalid_scope";
    }
}
