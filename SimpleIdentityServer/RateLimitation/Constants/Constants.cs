namespace SimpleIdentityServer.RateLimitation.Constants
{
    public static class RateLimitationConstants
    {
        public const string XRateLimitLimitName = "X-Rate-Limit-Limit";

        public const string XRateLimitRemainingName = "X-Rate-Limit-Remaining";

        public const string XRateLimitResetName = "X-Rate-Limit-Reset";

        public const string ErrorMessage = "Allow {0} requests per {1} minutes";
    }
}
