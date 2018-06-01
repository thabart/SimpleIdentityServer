using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Uma.Store.Redis
{
    public class RedisStoreModule : IModule
    {
        private const string _redisStorageConfiguration = "UmaRedisStorageConfiguration";
        private const string _redisStorageInstanceName = "UmaRedisStorageInstanceName";
        private const string _redisStoragePort = "UmaRedisStoragePort";

        private static readonly List<string> _configurationKeys = new List<string>
        {
            _redisStorageConfiguration,
            _redisStorageInstanceName
        };

        public void Configure(IApplicationBuilder applicationBuilder)
        {
        }

        public void Configure(IRouteBuilder routeBuilder)
        {
        }

        public void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder = null, IHostingEnvironment env = null, IDictionary<string, string> options = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var configuration = GetConfiguration(options);
            services.AddUmaRedisStore(configuration.Key, configuration.Value);
        }

        public IEnumerable<string> GetOptionKeys()
        {
            return new []
            {
                _redisStorageConfiguration,
                _redisStorageInstanceName,
                _redisStoragePort
            };
        }

        private static KeyValuePair<RedisCacheOptions, int> GetConfiguration(IDictionary<string, string> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var missingMandatoryConfs = _configurationKeys.Where(c => !options.ContainsKey(c));
            if (missingMandatoryConfs.Any())
            {
                throw new ModuleException("configuration", $"The {string.Join(",", missingMandatoryConfs)} configurations are missing");
            }

            int port = 6379;
            var b = options.ContainsKey(_redisStoragePort) && int.TryParse(options[_redisStoragePort], out port);
            return new KeyValuePair<RedisCacheOptions, int>(new RedisCacheOptions
            {
                Configuration = options[_redisStorageConfiguration],
                InstanceName = options[_redisStorageInstanceName]
            },
            port);
        }
    }
}
