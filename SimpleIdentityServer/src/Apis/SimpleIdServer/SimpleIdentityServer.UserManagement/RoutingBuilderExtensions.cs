using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace SimpleIdentityServer.UserManagement
{
    public static class RoutingBuilderExtensions
    {
        public static IRouteBuilder UseUserManagement(this IRouteBuilder routeBuilder)
        {
            if (routeBuilder == null)
            {
                throw new ArgumentNullException(nameof(routeBuilder));
            }

            routeBuilder.MapRoute("UserManagement",
                "User/{action}/{id?}",
                new { controller = "User", action = "Index", area = "UserManagement" },
                constraints: new { area = "UserManagement" });
            return routeBuilder;
        }
    }
}
