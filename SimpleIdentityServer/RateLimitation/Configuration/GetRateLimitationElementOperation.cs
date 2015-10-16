using System.Configuration;

namespace SimpleIdentityServer.RateLimitation.Configuration
{
    public interface IGetRateLimitationElementOperation
    {
        RateLimitationElement Execute(string rateLimitationElementName);

        bool IsEnabled();
    }

    public class GetRateLimitationElementOperation : IGetRateLimitationElementOperation
    {        
        public RateLimitationElement Execute(string rateLimitationElementName)
        {
            var rateLimitationSection = ConfigurationManager.GetSection(RateLimitationSection.SectionName) as RateLimitationSection;
            if (rateLimitationSection == null)
            {
                return null;
            }

            foreach (RateLimitationElement rateLimitation in rateLimitationSection.RateLimitations)
            {
                if (rateLimitation.Name.Equals(rateLimitationElementName))
                {
                    return rateLimitation;
                }
            }

            return null;
        }

        public bool IsEnabled()
        {
            var rateLimitationSection = ConfigurationManager.GetSection(RateLimitationSection.SectionName) as RateLimitationSection;
            if (rateLimitationSection == null)
            {
                return true;
            }

            return rateLimitationSection.IsEnabled;
        }
    }
}
