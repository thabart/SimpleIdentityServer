using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

using Microsoft.Practices.EnterpriseLibrary.Caching;
using RateLimitation.Models;
using RateLimitation.Constants;
using RateLimitation.Configuration;
using Microsoft.Practices.Unity;
using System.Web.Http.Controllers;

namespace RateLimitation.Attributes
{
    public class RateLimitationFilter : ActionFilterAttribute
    {
        private IGetRateLimitationElementOperation _getRateLimitationElementOperation;

        private bool _isEnabled;

        [Dependency]
        public IGetRateLimitationElementOperation GetRateLimitationElementOperation
        {
            set
            {
                _getRateLimitationElementOperation = value;
            }
        }
        
        /// <summary>
        /// Configure the rate limitation via configuration file.
        /// </summary>
        /// <param name="rateLimitationElementName"></param>
        public RateLimitationFilter()
        {
            _isEnabled = true;
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

        public override void OnActionExecuting(HttpActionContext httpActionContext)
        {
            LoadConfiguration();
            if (!_isEnabled)
            {
                return;
            }

            var today = DateTime.UtcNow;
            var responseCacheManager = CacheFactory.GetCacheManager();
            var owinContext = httpActionContext.Request.GetOwinContext();
            var ipAdress = owinContext.Request.RemoteIpAddress;
            var actionName = httpActionContext.ActionDescriptor.ActionName;
            var controllerName = httpActionContext.ControllerContext.ControllerDescriptor.ControllerName;

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

                httpActionContext.Response = httpActionContext.Request.CreateResponse(
                    (HttpStatusCode)429,
                    message);
                httpActionContext.Response.Headers.Add(RateLimitationConstants.XRateLimitLimitName, NumberOfRequests.ToString(CultureInfo.InvariantCulture));
                httpActionContext.Response.Headers.Add(RateLimitationConstants.XRateLimitRemainingName, numberOfRemainingRequests.ToString(CultureInfo.InvariantCulture));
                httpActionContext.Response.Headers.Add(RateLimitationConstants.XRateLimitResetName, timeResetTime.ToString(CultureInfo.InvariantCulture));
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext httpActionExecutedContext)
        {
            LoadConfiguration();
            if (!_isEnabled)
            {
                return;
            }

            var today = DateTime.UtcNow;
            var responseCacheManager = CacheFactory.GetCacheManager();
            var owinContext = httpActionExecutedContext.Request.GetOwinContext();
            var request = httpActionExecutedContext.Request;
            var ipAdress = owinContext.Request.RemoteIpAddress;
            var actionContext = httpActionExecutedContext.ActionContext;
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

            httpActionExecutedContext.Response.Headers.Add(RateLimitationConstants.XRateLimitLimitName, NumberOfRequests.ToString(CultureInfo.InvariantCulture));
            httpActionExecutedContext.Response.Headers.Add(RateLimitationConstants.XRateLimitRemainingName, numberOfRemainingRequests.ToString(CultureInfo.InvariantCulture));
            httpActionExecutedContext.Response.Headers.Add(RateLimitationConstants.XRateLimitResetName, timeResetTime.ToString(CultureInfo.InvariantCulture));
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
