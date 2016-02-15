using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Api
{
    public class Global : HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ConfigureUnity();
        }

        private static void ConfigureUnity()
        {
            var logging = SimpleIdentityServerEventSource.Log;
            var container = UnityConfig.Create(logging);
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}