using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace SimpleIdentityServer.Authenticate.Basic
{
    public static class RoutingBuilderExtensions
    {
        public static IRouteBuilder UseUserPasswordAuthentication(this IRouteBuilder routeBuilder)
        {
            if (routeBuilder == null)
            {
                throw new ArgumentNullException(nameof(routeBuilder));
            }

            routeBuilder.MapRoute("Authentication",
                "Authenticate/{action}/{id?}",
                new { controller = "Authenticate", action = "Index", area = "Authentication" },
                constraints: new { area = "Authentication" });
            return routeBuilder;
        }
    }
}
