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

using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Core.Stores;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace SimpleIdentityServer.Store.Redis
{
    public static class SimpleIdentityServerCoreExtensions
    {
        public static IServiceCollection AddRedisStores(this IServiceCollection serviceCollection, Action<RedisCacheOptions> callback, int port = 6379)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var options = new RedisCacheOptions();
            callback(options);
            return AddRedisStores(serviceCollection, options, port);
        }

        public static IServiceCollection AddRedisStores(this IServiceCollection serviceCollection, RedisCacheOptions options, int port = 6379)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!IsIpAddress(options.Configuration))
            {
                IPHostEntry ip = Dns.GetHostEntryAsync(options.Configuration).Result;
                var ipAddress = string.Empty;
                foreach (var adr in ip.AddressList)
                {
                    var strIp = adr.ToString();
                    if (IsIpAddress(strIp))
                    {
                        ipAddress = strIp;
                        break;
                    }
                }

                options.Configuration = ipAddress;
            }

            var redisStorage = new RedisStorage(options, port);
            serviceCollection.AddSingleton<IAuthorizationCodeStore>(new RedisAuthorizationCodeStore(redisStorage));
            serviceCollection.AddSingleton<ITokenStore>(new RedisTokenStore(redisStorage));
            return serviceCollection;
        }

        private static bool IsIpAddress(string host)
        {
            string ipPattern = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
            return Regex.IsMatch(host, ipPattern);
        }
    }
}
