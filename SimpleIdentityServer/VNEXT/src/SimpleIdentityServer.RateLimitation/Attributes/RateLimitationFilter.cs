using System;
using System.Globalization;

using Microsoft.Practices.Unity;

using SimpleIdentityServer.RateLimitation.Configuration;
using SimpleIdentityServer.RateLimitation.Constants;
using SimpleIdentityServer.RateLimitation.Models;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Net.Http.Headers;

namespace SimpleIdentityServer.RateLimitation.Attributes
{
    public class RateLimitationFilter : ActionFilterAttribute
    {
        private IGetRateLimitationElementOperation _getRateLimitationElementOperation;

        private ICacheManagerProvider _cacheManagerProvider;

        private bool _isEnabled;
        
        [Dependency]
        public IGetRateLimitationElementOperation GetRateLimitationElementOperation
        {
            set
            {
                _getRateLimitationElementOperation = value;
            }
        }

        [Dependency]
        public ICacheManagerProvider CacheManagerProvider
        {
            set { _cacheManagerProvider = value; }
        }
        
        /// <summary>
        /// Configure the rate limitation via configuration file.
        /// </summary>
        public RateLimitationFilter(
            IGetRateLimitationElementOperation getRateLimitationElementOperation,
            ICacheManagerProvider cacheManagerProvider,
            string rateLimitationElementName)
        {
            _isEnabled = true;
            _cacheManagerProvider = cacheManagerProvider;
            _getRateLimitationElementOperation = getRateLimitationElementOperation;
            RateLimitationElementName = rateLimitationElementName;
        }

        /// <summary>
        /// Configure the filter
        /// </summary>
        /// <param name="numberOfRequests"></param>
        /// <param name="slidingTime"></param>
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

        public string RateLimitationElementName { get; set; }

        public override void OnActionExecuting(ActionExecutingContext actionExecutingContext)
        {
            LoadConfiguration();
            if (!_isEnabled)
            {
                return;
            }

            var today = DateTime.UtcNow;
            var responseCacheManager = _cacheManagerProvider.GetCacheManager();
            HttpContext httpContext = actionExecutingContext.HttpContext;
            var connectionFeature = httpContext.Features.Get<IHttpConnectionFeature>();
            var ipAddress = connectionFeature == null ? string.Empty : connectionFeature.RemoteIpAddress.ToString();
            var actionName = actionExecutingContext.RouteData.Values["Action"].ToString();
            var controllerName = actionExecutingContext.RouteData.Values["Controller"].ToString();
            var record = new CacheableResponse
            {
                ActionName = actionName,
                ControllerName = controllerName,
                IpAdress = ipAddress    
            };
            var jsonResponse = record.GetIdentifier();
            var cachedData = responseCacheManager.GetData(jsonResponse);
            if (cachedData == null)
            {
                return;
            }

            var cacheableResponse = cachedData as CacheableResponse;
            if (cacheableResponse == null)
            {
                return;
            }
            
            var needToRefresh = cacheableResponse.UpdateDateTime.AddMinutes(SlidingTime) <= today;
            if (!needToRefresh && cacheableResponse.NumberOfRequests >= NumberOfRequests)
            {
                var numberOfRemainingRequests = 0;
                var timeResetTime = (cacheableResponse.UpdateDateTime.AddMinutes(SlidingTime) - today).TotalSeconds;
                var message = string.Format(
                        RateLimitationConstants.ErrorMessage,
                        NumberOfRequests.ToString(CultureInfo.InvariantCulture),
                        SlidingTime.ToString(CultureInfo.InvariantCulture));

                var headers = actionExecutingContext.HttpContext.Response.Headers;

                actionExecutingContext.Result = new ContentResult
                {
                    StatusCode = 429,
                    Content = message,
                    ContentType = new MediaTypeHeaderValue("text/plain")
                };
                headers.Add(RateLimitationConstants.XRateLimitLimitName, NumberOfRequests.ToString(CultureInfo.InvariantCulture));
                headers.Add(RateLimitationConstants.XRateLimitRemainingName, numberOfRemainingRequests.ToString(CultureInfo.InvariantCulture));
                headers.Add(RateLimitationConstants.XRateLimitResetName, timeResetTime.ToString(CultureInfo.InvariantCulture));
            }
        }

        public override void OnActionExecuted(ActionExecutedContext httpActionExecutedContext)
        {
            LoadConfiguration();
            if (!_isEnabled)
            {
                return;
            }

            var today = DateTime.UtcNow;
            var responseCacheManager = _cacheManagerProvider.GetCacheManager();
            var connectionFeature = httpActionExecutedContext.HttpContext.Features.Get<IHttpConnectionFeature>();
            var ipAddress = connectionFeature == null ? string.Empty : connectionFeature.RemoteIpAddress.ToString();
            var actionName = httpActionExecutedContext.RouteData.Values["Action"].ToString();
            var controllerName = httpActionExecutedContext.RouteData.Values["Controller"].ToString();

            var record = new CacheableResponse
            {
                ActionName = actionName,
                ControllerName = controllerName,
                IpAdress = ipAddress
            };
            var jsonResponse = record.GetIdentifier();
            var cachedData = responseCacheManager.GetData(jsonResponse);
            // Add into the cache
            if (cachedData == null)
            {
                record.UpdateDateTime = today;
                record.NumberOfRequests = 1;
                responseCacheManager.Add(jsonResponse, record);
            }
            else
            {
                // Update the cache.
                record = cachedData as CacheableResponse;
                if (record == null)
                {
                    return;
                }

                responseCacheManager.Remove(jsonResponse);
                var needToRefresh = record.UpdateDateTime.AddMinutes(SlidingTime) <= today;
                if (needToRefresh)
                {
                    record.UpdateDateTime = today;
                    record.NumberOfRequests = 1;
                }
                else
                {
                    record.NumberOfRequests++;
                }

                responseCacheManager.Add(jsonResponse, record);
            }            

            // Return the HTTP-HEADERS
            var numberOfRemainingRequests = NumberOfRequests - record.NumberOfRequests;
            if (numberOfRemainingRequests < 0)
            {
                numberOfRemainingRequests = 0;
            }

            var timeResetTime = (record.UpdateDateTime.AddMinutes(SlidingTime) - today).TotalSeconds;

            var response = httpActionExecutedContext.HttpContext.Response;
            if (response == null)
            {
                return;
            }

            response.Headers.Add(RateLimitationConstants.XRateLimitLimitName, NumberOfRequests.ToString(CultureInfo.InvariantCulture));
            response.Headers.Add(RateLimitationConstants.XRateLimitRemainingName, numberOfRemainingRequests.ToString(CultureInfo.InvariantCulture));
            response.Headers.Add(RateLimitationConstants.XRateLimitResetName, timeResetTime.ToString(CultureInfo.InvariantCulture));
        }

        private void LoadConfiguration()
        {
            _isEnabled = _getRateLimitationElementOperation.IsEnabled();
            if (string.IsNullOrWhiteSpace(RateLimitationElementName))
            {
                return;
            }

            var limitationElement = _getRateLimitationElementOperation.Execute(RateLimitationElementName);
            if (limitationElement == null)
            {
                return;
            }

            NumberOfRequests = limitationElement.NumberOfRequests;
            SlidingTime = limitationElement.SlidingTime;
        }
    }
}
