using System;
using System.Net.Http;
using System.Web.Http.Filters;

using Microsoft.Practices.EnterpriseLibrary.Caching;
using System.Globalization;
using System.Net;

namespace SimpleIdentityServer.Api.Attributes
{
    public class RateLimitationFilter : ActionFilterAttribute
    {
        private const string _xRateLimitLimitName = "X-Rate-Limit-Limit";

        private const string _xRateLimitRemainingName = "X-Rate-Limit-Remaining";

        private const string _xRateLimitResetName = "X-Rate-Limit-Reset";

        private const string _errorMessage = "Allow {0} requests per {1} minutes";

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
            var today = DateTime.UtcNow;

            var responseCacheManager = CacheFactory.GetCacheManager();
            var owinContext = httpActionExecutedContext.Request.GetOwinContext();
            var response = httpActionExecutedContext.Response;
            var request = httpActionExecutedContext.Request;
            var actionContext = httpActionExecutedContext.ActionContext;
            var ipAdress = owinContext.Request.RemoteIpAddress;
            var actionName = actionContext.ActionDescriptor.ActionName;
            var controllerName = actionContext.ControllerContext.ControllerDescriptor.ControllerName;

            var record = new CacheableResponse
            {
                ActionName = actionName,
                ControllerName = controllerName,
                IpAdress = ipAdress
            };
            var jsonResponse = record.GetIdentifier();
            var cachedData = responseCacheManager.GetData(jsonResponse);
            
            if (cachedData == null)
            {
                record.UpdateDateTime = today;
                record.NumberOfRequests = 1;
                responseCacheManager.Add(jsonResponse, record);
                return;
            }

            var cacheableResponse = cachedData as CacheableResponse;
            if (cacheableResponse == null)
            {
                return;
            }

            responseCacheManager.Remove(jsonResponse);
            var needToRefresh = cacheableResponse.UpdateDateTime.AddMinutes(SlidingTime) <= today;
            if (needToRefresh)
            {
                cacheableResponse.UpdateDateTime = today;
                cacheableResponse.NumberOfRequests = 1;
            }
            else
            {
                cacheableResponse.NumberOfRequests++;
            }

            responseCacheManager.Add(jsonResponse, cacheableResponse);

            var numberOfRemainingRequests = NumberOfRequests - cacheableResponse.NumberOfRequests;
            if (numberOfRemainingRequests < 0)
            {
                numberOfRemainingRequests = 0;
            }

            var timeResetTime = (cacheableResponse.UpdateDateTime.AddMinutes(SlidingTime) - today).TotalSeconds;

            response.Headers.Add(_xRateLimitLimitName, NumberOfRequests.ToString(CultureInfo.InvariantCulture));
            response.Headers.Add(_xRateLimitRemainingName, numberOfRemainingRequests.ToString(CultureInfo.InvariantCulture));
            response.Headers.Add(_xRateLimitResetName, timeResetTime.ToString(CultureInfo.InvariantCulture));

            if (cacheableResponse.NumberOfRequests <= NumberOfRequests)
            {
                return;
            }

            var message = string.Format(
                    _errorMessage,
                    NumberOfRequests.ToString(CultureInfo.InvariantCulture),
                    SlidingTime.ToString(CultureInfo.InvariantCulture));
            httpActionExecutedContext.Response = request.CreateResponse(
                (HttpStatusCode)429,
                message);
        }
    }
}