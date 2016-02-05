using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;

using Microsoft.Owin;
using Microsoft.Owin.Security.MicrosoftAccount;
using Microsoft.Practices.Unity;

using Owin;

using SimpleIdentityServer.Api.Configuration;

using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity;
using SimpleIdentityServer.Logging;

#if FAKE
using SimpleIdentityServer.DataAccess.Fake;
#endif

[assembly: OwinStartup(typeof(SimpleIdentityServer.Api.Startup))]

namespace SimpleIdentityServer.Api
{
    public class Startup
    {
        private static bool _isInitialized;

        public void Configuration(IAppBuilder app)
        {
            // TODO : check this blog 
            // http://weblog.west-wind.com/posts/2015/Apr/29/Adding-minimal-OWIN-Identity-Authentication-to-an-Existing-ASPNET-MVC-Application
            
            var logging = SimpleIdentityServerEventSource.Log;

            var httpConfiguration = new HttpConfiguration();
            if (_isInitialized == false)
            {
                ConfigureUnity(httpConfiguration, logging);
#if FAKE
                PopupulateFakeDataSource();
#endif
                _isInitialized = true;
            }

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);


            var options = new MicrosoftAccountAuthenticationOptions
            {
                ClientId = "0000000048185530",
                ClientSecret = "KN12jxYIAYOr0bCLXFBcXhBrTlZyLNAZ",
                CallbackPath = new PathString("/Authenticate/ExternalLoginCallback"),
                Provider = new MicrosoftAccountAuthenticationProvider()
                {
                    OnAuthenticated = (context) =>
                    {
                        return Task.FromResult(0);
                    }
                }
            };
            app.UseMicrosoftAccountAuthentication(options);

            SwaggerConfig.Configure(httpConfiguration);
            WebApiConfig.Register(httpConfiguration, app, logging);
        }

#if FAKE
        private static void PopupulateFakeDataSource()
        {
            FakeDataSource.Instance().Clients = Clients.Get();
            FakeDataSource.Instance().Scopes = Scopes.Get();
            FakeDataSource.Instance().ResourceOwners = ResourceOwners.Get();
            FakeDataSource.Instance().JsonWebKeys = JsonWebKeys.Get();
            FakeDataSource.Instance().Translations = Translations.Get();
        }
#endif

        private static void ConfigureUnity(HttpConfiguration httpConfiguration, ISimpleIdentityServerEventSource simpleIdentityServerEventSource)
        {
            var container = UnityConfig.Create(simpleIdentityServerEventSource);
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
