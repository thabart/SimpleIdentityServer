using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;

namespace SimpleIdentityServer.Host.Extensions
{
    public static class UriHelperExtensions 
    {
        private static IHttpContextAccessor HttpContextAccessor;
        
        public static void Configure(IHttpContextAccessor httpContextAccessor) 
        {
            HttpContextAccessor = httpContextAccessor;
        }
        
        public static string AbsoluteAction(this IUrlHelper urlHelper, string actionName, string controllerName, object routeValues = null) 
        {
            var scheme  = HttpContextAccessor.HttpContext.Request.Scheme;
            return urlHelper.Action(actionName, controllerName, routeValues, scheme);
        }
    }
}