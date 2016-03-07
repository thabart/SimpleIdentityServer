#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Net.Http.Headers;
using SimpleIdentityServer.RateLimitation.Configuration;
using SimpleIdentityServer.RateLimitation.Constants;
using SimpleIdentityServer.RateLimitation.Models;
using System;
using System.Globalization;
using System.Text;

namespace SimpleIdentityServer.RateLimitation.Attributes
{
    public class RateLimitationFilter : ActionFilterAttribute, IFilterMetadata
    {
        private IGetRateLimitationElementOperation _getRateLimitationElementOperation;

        private RateLimitationOptions _rateLimitationOptions;

        private bool _isEnabled;

        #region Constructor

        /// <summary>
        /// Configure the rate limitation via configuration file.
        /// </summary>
        public RateLimitationFilter(
            IGetRateLimitationElementOperation getRateLimitationElementOperation,
            IOptions<RateLimitationOptions> rateLimitationOptions,
            string rateLimitationElementName)
        {
            if (getRateLimitationElementOperation == null)
            {
                throw new ArgumentNullException(nameof(getRateLimitationElementOperation));
            }

            if (rateLimitationOptions == null || rateLimitationOptions.Value == null)
            {
                throw new ArgumentNullException(nameof(rateLimitationOptions));
            }

            _isEnabled = true;
            _getRateLimitationElementOperation = getRateLimitationElementOperation;
            _rateLimitationOptions = rateLimitationOptions.Value;
            RateLimitationElementName = rateLimitationElementName;
        }

        #endregion

        #region Properties

        // [Dependency]
        public IGetRateLimitationElementOperation GetRateLimitationElementOperation
        {
            set
            {
                _getRateLimitationElementOperation = value;
            }
        }

        // [Dependency]
        public RateLimitationOptions RateLimitationOptions
        {
            set { _rateLimitationOptions = value; }
        }

        public int NumberOfRequests { get; set; }

        /// <summary>
        /// Sliding time in minutes.
        /// </summary>
        public double SlidingTime { get; set; }

        public string RateLimitationElementName { get; set; }

        #endregion

        #region Public methods

        public override void OnActionExecuting(ActionExecutingContext actionExecutingContext)
        {
            LoadConfiguration();
            if (!_isEnabled)
            {
                return;
            }

            var today = DateTime.UtcNow;
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
            var cachedData = GetValue(jsonResponse);
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
            var cachedData = GetValue(jsonResponse);
            // Add into the cache
            if (cachedData == null)
            {
                record.UpdateDateTime = today;
                record.NumberOfRequests = 1;
                SetValue(jsonResponse, record);
            }
            else
            {
                // Update the cache.
                record = cachedData as CacheableResponse;
                if (record == null)
                {
                    return;
                }

                Remove(jsonResponse);
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

                SetValue(jsonResponse, record);
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

        #endregion

        #region Private methods

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

        private object GetValue(string key)
        {
            var inMemoryRateLimitationOptions = _rateLimitationOptions as InMemoryRateLimitationOptions;
            if (inMemoryRateLimitationOptions != null)
            {
                object result = null;
                inMemoryRateLimitationOptions.MemoryCache.TryGetValue(key, out result);
                return result;
            }

            var distributedRateLimitationOptions = _rateLimitationOptions as DistributedRateLimitationOptions;
            if (distributedRateLimitationOptions != null)
            {
                var bytes = distributedRateLimitationOptions.DistributedCache.Get(key);
                return Encoding.UTF8.GetString(bytes);
            }

            return null;
        }

        private void SetValue(string key, object value)
        {
            var inMemoryRateLimitationOptions = _rateLimitationOptions as InMemoryRateLimitationOptions;
            if (inMemoryRateLimitationOptions != null)
            {
                inMemoryRateLimitationOptions.MemoryCache.Set(key, 
                    value,
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(SlidingTime)));
                return;
            }

            var distributedRateLimitationOptions = _rateLimitationOptions as DistributedRateLimitationOptions;
            if (distributedRateLimitationOptions != null)
            {
                /*
                distributedRateLimitationOptions.DistributedCache.Set(key,
                    Encoding.UTF8.GetBytes(value),
                    new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(SlidingTime)));
                */
            }
        }

        private void Remove(string key)
        {
            var inMemoryRateLimitationOptions = _rateLimitationOptions as InMemoryRateLimitationOptions;
            if (inMemoryRateLimitationOptions != null)
            {
                inMemoryRateLimitationOptions.MemoryCache.Remove(key);
                return;
            }

            var distributedRateLimitationOptions = _rateLimitationOptions as DistributedRateLimitationOptions;
            if (distributedRateLimitationOptions != null)
            {
                distributedRateLimitationOptions.DistributedCache.Remove(key);
            }
        }

        #endregion
    }
}
