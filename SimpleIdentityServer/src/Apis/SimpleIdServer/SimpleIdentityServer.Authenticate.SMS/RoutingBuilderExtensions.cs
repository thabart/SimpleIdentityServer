using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace SimpleIdentityServer.Authenticate.SMS
{
    public static class RoutingBuilderExtensions
    {
        public static IRouteBuilder UseSmsAuthentication(this IRouteBuilder routeBuilder)
        {
            if (routeBuilder == null)
            {
                throw new ArgumentNullException(nameof(routeBuilder));
            }
            
            routeBuilder.MapRoute("BasicAuthentication",
                "Authenticate/{action}/{id?}",
                new { controller = "Authenticate", action = "Index", area = "AuthSMS" },
                constraints: new { area = "AuthSMS" });
            return routeBuilder;
        }
    }
}
