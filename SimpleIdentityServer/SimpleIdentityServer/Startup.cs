using System.Linq;
using System.Web.Http;
using System.Web.Http.Filters;

using Microsoft.Owin;
using Microsoft.Practices.Unity;

using Owin;

using SimpleIdentityServer.Api.Configuration;
using SimpleIdentityServer.DataAccess.Fake;

using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity;

[assembly: OwinStartup(typeof(SimpleIdentityServer.Api.Startup))]

namespace SimpleIdentityServer.Api
{
    public class Startup
    {
        private static bool _isInitialized = false;

        public void Configuration(IAppBuilder app)
        {
            // TODO : check this blog 
            // http://weblog.west-wind.com/posts/2015/Apr/29/Adding-minimal-OWIN-Identity-Authentication-to-an-Existing-ASPNET-MVC-Application

            var httpConfiguration = new HttpConfiguration();
            if (_isInitialized == false)
            {
                ConfigureUnity(httpConfiguration);
                PopupulateFakeDataSource();
                _isInitialized = true;
            }

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
            });

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
