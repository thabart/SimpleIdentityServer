using System.Web.Http;

using Owin;

using SimpleIdentityServer.Api.Attributes;

namespace SimpleIdentityServer.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config, IAppBuilder appBuilder)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{id}",
                new
                {
                    id = RouteParameter.Optional
                }
            );

            config.Filters.Add(new IdentityServerExceptionFilter());

            appBuilder.UseWebApi(config);
        }
    }
}
