using System.Linq;
using System.Web.Http.Filters;

using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.Unity;

using System.Web.Http;
using SimpleIdentityServer.Api.Configuration;
using SimpleIdentityServer.Core.Protector;

namespace SimpleIdentityServer.Api
{
    public static class UnityConfig
    {
        public static void Configure(HttpConfiguration httpConfiguration)
        {
            var container = new UnityContainer();

            container.RegisterType<ICacheManager, CacheManager>();
            container.RegisterType<ICertificateStore, CertificateStore>();

            ModuleLoader.LoadContainer(container);

            RegisterFilterInjector(httpConfiguration, container);

            httpConfiguration.DependencyResolver = new UnityResolver(container);
        }

        private static void RegisterFilterInjector(HttpConfiguration config, UnityContainer container)
        {
            //Register the filter injector
            var providers = config.Services.GetFilterProviders().ToList();
            var defaultprovider = providers.Single(i => i is ActionDescriptorFilterProvider);
            config.Services.Remove(typeof(IFilterProvider), defaultprovider);
            config.Services.Add(typeof(IFilterProvider), new UnityFilterProvider(container));
        }
    }
}