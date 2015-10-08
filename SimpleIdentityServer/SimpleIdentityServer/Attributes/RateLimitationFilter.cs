using System.Net.Http;
using System.Web.Http.Filters;

namespace SimpleIdentityServer.Api.Attributes
{
    public class RateLimitationFilter : ActionFilterAttribute
    {
        public RateLimitationFilter()
        {
        }

        public RateLimitationFilter(
            int numberOfRequests,
            int period)
        {
            NumberOfRequests = numberOfRequests;
            Period = period;
        }

        public int NumberOfRequests { get; set; }

        public int Period { get; set; }
        
        public override void OnActionExecuted(HttpActionExecutedContext httpActionExecutedContext)
        {
            // store ip & action name & controller & end-date = current-date + period & number of requests
            var ip = httpActionExecutedContext.Request.GetOwinContext().Request.RemoteIpAddress;
            var actionName = httpActionExecutedContext.ActionContext.ActionDescriptor.ActionName;
            var controller = httpActionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName;

            // In the header response returns :
            // X-Rate-Limit-Limit
            // X-Rate-Limit-Remaining
            // X-Rate-Limit-Rest

            // For the cache use the library : EnterpriseLibrary.Caching, useful for isolated storage ==> scalability
            string s = "zz";
        }
    }
}