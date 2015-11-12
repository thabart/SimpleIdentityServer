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

            config.Routes.MapHttpRoute("DiscoveryRoute",
                Constants.EndPoints.DiscoveryAction,
                new
                {
                    controller = "Discovery",
                    action = "Get"
                });

            config.Routes.MapHttpRoute("AuthorizationRoute",
                Constants.EndPoints.Authorization,
                new
                {
                    controller = "Authorization",
                    action = "Get"
                });

            config.Routes.MapHttpRoute("TokenRoute",
                Constants.EndPoints.Token,
                new
                {
                    controller = "Token",
                    action = "Post"
                });

            config.Filters.Add(new IdentityServerExceptionFilter());

            appBuilder.UseWebApi(config);
        }
    }
}
