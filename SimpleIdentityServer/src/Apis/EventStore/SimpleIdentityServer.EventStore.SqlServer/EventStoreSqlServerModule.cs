using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.EF.SqlServer
{
    public class EventStoreSqlServerModule : IModule
    {
        private const string _evtConnectionString = "EventStoreConnectionString";

        public void Configure(IApplicationBuilder applicationBuilder)
        {
        }

        public void Configure(IRouteBuilder routeBuilder)
        {
        }

        public void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder = null, IHostingEnvironment env = null,  IDictionary<string, string> options = null, AuthenticationBuilder authBuilder = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!options.ContainsKey(_evtConnectionString))
            {
                throw new ModuleException("configuration", $"The {_evtConnectionString} configuration is missing");
            }

            services.AddEventStoreSqlServerEF(options[_evtConnectionString]);
        }

        public IEnumerable<string> GetOptionKeys()
        {
            return new[]
            {
                _evtConnectionString
            };
        }
    }
}
