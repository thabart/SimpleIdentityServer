using System.Web.Http;

using Microsoft.Owin;
using Owin;

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
                _isInitialized = true;
            }

            SwaggerConfig.Configure(httpConfiguration);
            WebApiConfig.Register(httpConfiguration, app);
        }
    }
}
