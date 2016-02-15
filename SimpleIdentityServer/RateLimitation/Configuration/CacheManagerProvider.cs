using Microsoft.Practices.EnterpriseLibrary.Caching;

namespace SimpleIdentityServer.RateLimitation.Configuration
{
    public interface ICacheManagerProvider
    {
        ICacheManager GetCacheManager();
    }

    public class CacheManagerProvider : ICacheManagerProvider
    {
        public ICacheManager GetCacheManager()
        {
            return CacheFactory.GetCacheManager();
        }
    }
}
