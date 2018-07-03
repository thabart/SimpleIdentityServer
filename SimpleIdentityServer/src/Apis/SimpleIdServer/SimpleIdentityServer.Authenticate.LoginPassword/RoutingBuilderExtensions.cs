using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace SimpleIdentityServer.Authenticate.LoginPassword
{
    public static class RoutingBuilderExtensions
    {
        public static IRouteBuilder UseLoginPasswordAuthentication(this IRouteBuilder routeBuilder)
        {
            if (routeBuilder == null)
            {
                throw new ArgumentNullException(nameof(routeBuilder));
            }
            
            routeBuilder.MapRoute("PasswordAuthentication",
                "Authenticate/{action}/{id?}",
                new { controller = "Authenticate", action = "Index", area = "pwd" },
                constraints: new { area = "pwd" });
            return routeBuilder;
        }
    }
}
