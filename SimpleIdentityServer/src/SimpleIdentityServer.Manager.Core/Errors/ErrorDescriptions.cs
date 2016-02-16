namespace SimpleIdentityServer.Manager.Core.Errors
{
    public static class ErrorDescriptions
    {
        public const string TheUrlIsNotWellFormed = "the url {0} is not well formed";

        public const string TheTokenIsNotAValidJws = "the token is not a valid JWS";

        public const string TheJsonWebKeyCannotBeFound = "the json web key {0} cannot be found {1}";
    }
}
