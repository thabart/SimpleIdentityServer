using System.Web.Http;

using Microsoft.Owin;

using Owin;

[assembly: OwinStartup(typeof(SimpleIdentityServer.Api.Startup))]

namespace SimpleIdentityServer.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var httpConfiguration = new HttpConfiguration();
            UnityConfig.Configure(httpConfiguration);
            WebApiConfig.Register(httpConfiguration, app);
        }
    }
}
