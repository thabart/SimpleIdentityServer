using System.Configuration;

namespace RateLimitation.Configuration
{
    public class RateLimitationCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new RateLimitationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RateLimitationElement)element).Name;
        }
    }
}
