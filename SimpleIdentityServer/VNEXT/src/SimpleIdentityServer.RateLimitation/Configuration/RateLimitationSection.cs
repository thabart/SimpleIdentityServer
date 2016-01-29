using System.Configuration;

namespace SimpleIdentityServer.RateLimitation.Configuration
{
    public class RateLimitationSection : ConfigurationSection
    {
        public const string SectionName = "RateLimitationSection";

        [ConfigurationProperty("isEnabled", DefaultValue = true, IsRequired = false)]
        public bool IsEnabled
        {
            get
            {
                return (bool)this["isEnabled"];
            } set
            {
                this["isEnabled"] = value;
            }
        }

        [ConfigurationProperty("RateLimitations")]
        [ConfigurationCollection(typeof(RateLimitationCollection), AddItemName = "add")]
        public RateLimitationCollection RateLimitations
        {
            get
            {
                return (RateLimitationCollection)base["RateLimitations"];
            }
        }
    }
}
