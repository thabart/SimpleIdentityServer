using Microsoft.Practices.EnterpriseLibrary.Caching;
using SimpleIdentityServer.RateLimitation.Configuration;

namespace SimpleIdentityServer.Api.Tests.Common.Fakes
{
    public class FakeCacheManagerProvider : ICacheManagerProvider
    {
        private ICacheManager _cacheManager;

        public ICacheManager CacheManager
        {
            set { _cacheManager = value; }
        }

        public ICacheManager GetCacheManager()
        {
            return _cacheManager;
        }
    }
}
