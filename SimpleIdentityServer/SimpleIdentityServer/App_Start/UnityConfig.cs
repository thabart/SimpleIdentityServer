using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.Unity;

using SimpleIdentityServer.Api.Configuration;
using SimpleIdentityServer.Core.Protector;

namespace SimpleIdentityServer.Api
{
    public static class UnityConfig
    {
        public static UnityContainer Create()
        {
            var container = new UnityContainer();

            container.RegisterType<ICacheManager, CacheManager>();
            container.RegisterType<ICertificateStore, CertificateStore>();

            ModuleLoader.LoadContainer(container);

            return container;
        }
    }
}