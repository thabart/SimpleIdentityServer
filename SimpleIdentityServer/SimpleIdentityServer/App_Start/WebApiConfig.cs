using System.Web.Http;

using Owin;

using SimpleIdentityServer.Api.Attributes;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Api
{
    public static class WebApiConfig
    {
        public static void Register(
            HttpConfiguration config, 
            IAppBuilder appBuilder,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource)
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

            config.Routes.MapHttpRoute("UserInfoRoute",
                Constants.EndPoints.UserInfo,
                new
                {
                    controller = "UserInfo"
                });

            config.Routes.MapHttpRoute("JwksRoute",
                Constants.EndPoints.Jwks,
                new
                {
                    controller = "Jwks"
                });

            config.Routes.MapHttpRoute("RegisterRoute",
                Constants.EndPoints.Registration,
                new
                {
                    controller = "Registration",
                    action = "Post"
                });

            config.Filters.Add(new IdentityServerExceptionFilter(simpleIdentityServerEventSource));

            appBuilder.UseWebApi(config);
        }
    }
}
