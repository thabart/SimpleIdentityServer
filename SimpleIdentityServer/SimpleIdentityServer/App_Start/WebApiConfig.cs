using System.Web.Http;

using Microsoft.Owin.Security.OAuth;

using Owin;

namespace SimpleIdentityServer.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config, IAppBuilder appBuilder)
        {
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            appBuilder.UseWebApi(config);
        }
    }
}
