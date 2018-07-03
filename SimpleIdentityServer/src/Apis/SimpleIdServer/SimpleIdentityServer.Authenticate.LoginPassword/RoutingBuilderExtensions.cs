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
            
            routeBuilder.MapRoute("BasicAuthentication",
                "Authenticate/{action}/{id?}",
                new { controller = "Authenticate", action = "Index", area = "AuthLoginPassword" },
                constraints: new { area = "AuthLoginPassword" });
            return routeBuilder;
        }
    }
}
