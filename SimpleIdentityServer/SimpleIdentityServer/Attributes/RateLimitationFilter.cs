using System;
using System.Net.Http;
using System.Web.Http.Filters;

using Microsoft.Practices.EnterpriseLibrary.Caching;

namespace SimpleIdentityServer.Api.Attributes
{
    public class RateLimitationFilter : ActionFilterAttribute
    {
        public RateLimitationFilter()
        {
        }

        public RateLimitationFilter(
            int numberOfRequests,
            double slidingTime)
        {
            NumberOfRequests = numberOfRequests;
            SlidingTime = slidingTime;
        }

        public int NumberOfRequests { get; set; }

        /// <summary>
        /// Sliding time in minutes.
        /// </summary>
        public double SlidingTime { get; set; }
        
        public override void OnActionExecuted(HttpActionExecutedContext httpActionExecutedContext)
        {
            var responseCacheManager = CacheFactory.GetCacheManager();
            var owinContext = httpActionExecutedContext.Request.GetOwinContext();
            var actionContext = httpActionExecutedContext.ActionContext;
            var ipAdress = owinContext.Request.RemoteIpAddress;
            var actionName = actionContext.ActionDescriptor.ActionName;
            var controllerName = actionContext.ControllerContext.ControllerDescriptor.ControllerName;

            var response = new CacheableResponse
            {
                ActionName = actionName,
                ControllerName = controllerName,
                IpAdress = ipAdress
            };
            var jsonResponse = response.GetIdentifier();
            var cachedData = responseCacheManager.GetData(jsonResponse);
            if (cachedData == null)
            {
                response.UpdateDateTime = DateTime.Now;
                response.NumberOfRequests = 0;
                responseCacheManager.Add(jsonResponse, response);
                return;
            }

            var cacheableResponse = cachedData as CacheableResponse;
            if (cacheableResponse == null)
            {
                return;
            }

            responseCacheManager.Remove(jsonResponse);
            var needToRefresh = cacheableResponse.UpdateDateTime.AddMinutes(SlidingTime) <= DateTime.Now;
            responseCacheManager.Remove(jsonResponse);
            if (needToRefresh)
            {
                cacheableResponse.UpdateDateTime = DateTime.Now;
                cacheableResponse.NumberOfRequests = 0;
            }
            else
            {
                cacheableResponse.NumberOfRequests = cacheableResponse.NumberOfRequests + 1;
            }

            responseCacheManager.Add(jsonResponse, cacheableResponse);

            // store ip & action name & controller & end-date = current-date + period & number of requests

            // In the header response returns :
            // X-Rate-Limit-Limit
            // X-Rate-Limit-Remaining
            // X-Rate-Limit-Rest

            // For the cache use the library : EnterpriseLibrary.Caching, useful for isolated storage ==> scalability
        }
    }
}