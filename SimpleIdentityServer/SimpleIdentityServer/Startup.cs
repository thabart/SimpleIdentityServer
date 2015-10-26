using System.Linq;
using System.Web.Http;
using System.Web.Http.Filters;

using Microsoft.Owin;
using Microsoft.Practices.Unity;

using Owin;

using SimpleIdentityServer.Api.Configuration;
using SimpleIdentityServer.DataAccess.Fake;

[assembly: OwinStartup(typeof(SimpleIdentityServer.Api.Startup))]

namespace SimpleIdentityServer.Api
{
    public class Startup
    {
        private static bool _isInitialized = false;

        public void Configuration(IAppBuilder app)
        {
            var httpConfiguration = new HttpConfiguration();
            if (_isInitialized == false)
            {
                ConfigureUnity(httpConfiguration);
                PopupulateFakeDataSource();
                _isInitialized = true;
            }

            SwaggerConfig.Configure(httpConfiguration);
            WebApiConfig.Register(httpConfiguration, app);
        }

        private static void PopupulateFakeDataSource()
        {
            FakeDataSource.Instance().Clients = Clients.Get();
            FakeDataSource.Instance().Scopes = Scopes.Get();
            FakeDataSource.Instance().ResourceOwners = ResourceOwners.Get();
        }

        private static void ConfigureUnity(HttpConfiguration httpConfiguration)
        {
            var container = UnityConfig.Create();
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
