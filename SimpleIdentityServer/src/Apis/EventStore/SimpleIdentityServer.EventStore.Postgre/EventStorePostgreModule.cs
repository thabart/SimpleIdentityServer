using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.EF.Postgre
{
    public class EventStorePostgreModule : IModule
    {
        private const string _evtConnectionString = "EventStoreConnectionString";

        public void Configure(IApplicationBuilder applicationBuilder)
        {
        }

        public void Configure(IRouteBuilder routeBuilder)
        {
        }

        public void ConfigureAuthentication(AuthenticationBuilder authBuilder, IDictionary<string, string> options = null)
        {
        }

        public void ConfigureAuthorization(AuthorizationOptions authorizationOptions, IDictionary<string, string> options = null)
        {
        }

        public void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder = null, IHostingEnvironment env = null,  IDictionary<string, string> options = null, IEnumerable<ModuleUIDescriptor> moduleUiDescriptors = null)
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

            services.AddEventStorePostgresqlEF(options[_evtConnectionString]);
        }

        public ModuleUIDescriptor GetModuleUI()
        {
            return null;
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
