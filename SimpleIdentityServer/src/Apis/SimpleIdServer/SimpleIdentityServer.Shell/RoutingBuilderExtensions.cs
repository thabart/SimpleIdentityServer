using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace SimpleIdentityServer.Shell
{
    public static class RoutingBuilderExtensions
    {
        public static IRouteBuilder UseShell(this IRouteBuilder routeBuilder)
        {
            if (routeBuilder == null)
            {
                throw new ArgumentNullException(nameof(routeBuilder));
            }

            routeBuilder.MapRoute("Error401Route",
                Host.Constants.EndPoints.Get401,
                new
                {
                    controller = "Error",
                    action = "Get401",
                    area = "Shell"
                }, constraints: new { area = "Shell" });
            routeBuilder.MapRoute("Error404Route",
                Host.Constants.EndPoints.Get404,
                new
                {
                    controller = "Error",
                    action = "Get404",
                    area = "Shell"
                }, constraints: new { area = "Shell" });
            routeBuilder.MapRoute("Error500Route",
                Host.Constants.EndPoints.Get500,
                new
                {
                    controller = "Error",
                    action = "Get500",
                    area = "Shell"
                }, constraints: new { area = "Shell" });
            routeBuilder.MapRoute("BasicShellAuthentication",
                "{controller}/{action}/{id?}",
                new { controller = "Home", action = "Index", area = "Shell" }, constraints: new { area = "Shell" });
            return routeBuilder;
        }
    }
}
