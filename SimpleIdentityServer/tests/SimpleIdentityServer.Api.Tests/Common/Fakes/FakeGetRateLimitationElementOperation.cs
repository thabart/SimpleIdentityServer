using SimpleIdentityServer.RateLimitation.Configuration;

namespace SimpleIdentityServer.Api.Tests.Common.Fakes
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
