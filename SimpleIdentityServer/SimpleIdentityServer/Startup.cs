using System.Web.Http;

using Microsoft.Owin;
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
                UnityConfig.Configure(httpConfiguration);
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
    }
}
