using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.Unity;

using SimpleIdentityServer.Api.Configuration;
using SimpleIdentityServer.Api.Helpers;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Services;

namespace SimpleIdentityServer.Api
{
    public static class UnityConfig
    {
        public static UnityContainer Create()
        {
            var container = new UnityContainer();

            container.RegisterType<ICacheManager, CacheManager>();
            container.RegisterType<ICertificateStore, CertificateStore>();
            container.RegisterType<IResourceOwnerService, InMemoryUserService>();
            container.RegisterType<ISessionManager, SessionManager>();

            ModuleLoader.LoadContainer(container);

            return container;
        }
    }
}