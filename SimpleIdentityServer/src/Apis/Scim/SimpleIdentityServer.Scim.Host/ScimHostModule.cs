using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using SimpleIdentityServer.Scim.Host.Controllers;
using SimpleIdentityServer.Scim.Host.Extensions;

namespace SimpleIdentityServer.Scim.Host
{
    public class ScimHostModule : IModule
    {
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

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }
            
            services.AddScimHost();
            var assembly = typeof(SchemasController).Assembly;
            mvcBuilder.AddApplicationPart(assembly);
        }

        public IEnumerable<string> GetOptionKeys()
        {
            return null;
        }
    }
}
