using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Db.EF.Postgre
{
    public class ScimPostgreModule : IModule
    {
        private const string ScimConnectionString = "ScimConnectionString";

        public void Configure(IApplicationBuilder applicationBuilder)
        {
        }

        public void Configure(IRouteBuilder routeBuilder)
        {
        }

        public void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder = null, IHostingEnvironment env = null,  IDictionary<string, string> options = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(ScimConnectionString));
            }

            if (!options.ContainsKey(ScimConnectionString))
            {
                throw new ModuleException("configuration", $"The {ScimConnectionString} configuration is missing");
            }

            services.AddScimPostgresqlEF(options[ScimConnectionString]);
        }

        public IEnumerable<string> GetOptionKeys()
        {
            return new[]
            {
                ScimConnectionString
            };
        }
    }
}
