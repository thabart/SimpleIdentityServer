using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace WebApiContrib.Core.Storage.Redis
{
    public class RedisStorageModule : IModule
    {
        private const string _redisCacheInstanceName = "RedisCacheInstanceName";
        private const string _redisCacheConfiguration = "RedisCacheConfiguration";
        private const string _redisCachePort = "RedisCachePort";

        public void Configure(IApplicationBuilder applicationBuilder)
        {
        }

        public void Configure(IRouteBuilder routeBuilder)
        {
        }

        public void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder = null, IHostingEnvironment env = null, IDictionary<string, string> options = null, AuthenticationBuilder authBuilder = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!options.ContainsKey(_redisCacheConfiguration))
            {
                throw new ModuleException("configuration", $"The {_redisCacheConfiguration} configuration is missing");
            }

            if (!options.ContainsKey(_redisCacheInstanceName))
            {
                throw new ModuleException("configuration", $"The {_redisCacheInstanceName} configuration is missing");
            }

            int port = 6379;
            var b = options.ContainsKey(_redisCachePort) && int.TryParse(options[_redisCachePort], out port);
            var redisOptions = new RedisCacheOptions
            {
                Configuration = options[_redisCacheConfiguration],
                InstanceName = options[_redisCacheInstanceName]
            };
            var storage = new RedisStorage(redisOptions, port);
            var storageOptions = new StorageOptions
            {
                Storage = storage
            };
            services.AddSingleton<IStorage>(storage);
            services.AddSingleton(storageOptions);
        }

        public IEnumerable<string> GetOptionKeys()
        {
            return new[]
            {
                _redisCacheInstanceName,
                _redisCacheConfiguration,
                _redisCachePort
            };
        }
    }
}
