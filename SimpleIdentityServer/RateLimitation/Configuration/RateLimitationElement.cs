using System.Configuration;

namespace RateLimitation.Configuration
{
    public class RateLimitationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("numberOfRequests", IsRequired = true)]
        public int NumberOfRequests
        {
            get
            {
                return (int)this["numberOfRequests"];
            }
            set
            {
                this["numberOfRequests"] = value;
            }
        }

        [ConfigurationProperty("slidingTime", IsRequired = true)]
        public double SlidingTime
        {
            get
            {
                return (double)this["slidingTime"];
            }
            set
            {
                this["slidingTime"] = value;
            }
        }
    }
}
