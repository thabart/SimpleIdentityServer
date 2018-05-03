namespace SimpleIdentityServer.Manager.Core
{
    internal static class Constants
    {
        public static class ErrorCodes
        {
            public const string InvalidRequestCode = "invalid_request";
        }

        public static class ErrorDescriptions
        {
            public const string ClaimExists = "a claim already exists with the same name";
            public const string ClaimDoesntExist = "the claim doesn't exist";
        }
    }
}
