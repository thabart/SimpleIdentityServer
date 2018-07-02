using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace SimpleIdentityServer.Authenticate.Basic
{
    public static class RoutingBuilderExtensions
    {
        public static IRouteBuilder UseAuthentication(this IRouteBuilder routeBuilder)
        {
            if (routeBuilder == null)
            {
                throw new ArgumentNullException(nameof(routeBuilder));
            }

            routeBuilder.MapRoute("Authentication",
                "{area:exists}/Authenticate/{action}/{id?}",
                new { controller = "Authenticate", action = "Index" });
            routeBuilder.MapRoute("BasicAuthentication",
                "Authenticate/{action}/{id?}",
                new { controller = "Authenticate", action = "Index" });
            return routeBuilder;
        }
    }
}
