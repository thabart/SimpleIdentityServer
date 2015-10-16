using SimpleIdentityServer.RateLimitation.Configuration;

namespace SimpleIdentityServer.Api.Tests.Common
{
    public class FakeGetRateLimitationElementOperation : IGetRateLimitationElementOperation
    {

        public bool Enabled { get; set; }

        public RateLimitationElement RateLimitationElement { get; set; }

        public RateLimitationElement Execute(string rateLimitationElementName)
        {
            return RateLimitationElement;
        }

        public bool IsEnabled()
        {
            return Enabled;
        }
    }
}
