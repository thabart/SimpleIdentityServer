using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.EventStore.Handler
{
    public class EventStoreHandlerModule : IModule
    {
        private const string _typeKey = "EventStoreHandlerType";

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

            if (!options.ContainsKey(_typeKey))
            {
                throw new ModuleException("configuration", $"The {_typeKey} configuration is missing");
            }

            services.AddEventStoreBusHandler(new EventStoreHandlerOptions(options[_typeKey]));
        }

        public IEnumerable<string> GetOptionKeys()
        {
            return new[]
            {
                _typeKey
            };
        }
    }
}
