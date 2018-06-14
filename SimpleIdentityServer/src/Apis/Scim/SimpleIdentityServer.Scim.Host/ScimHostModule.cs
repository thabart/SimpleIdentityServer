using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using SimpleIdentityServer.Scim.Host.Controllers;
using SimpleIdentityServer.Scim.Host.Extensions;
using System;
using System.Collections.Generic;

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

        public void ConfigureAuthentication(AuthenticationBuilder authBuilder, IDictionary<string, string> options = null)
        {
        }

        public void ConfigureAuthorization(AuthorizationOptions authorizationOptions, IDictionary<string, string> options = null)
        {
            authorizationOptions.AddScimAuthPolicy();
        }

        public void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder = null, IHostingEnvironment env = null, IDictionary<string, string> options = null, IEnumerable<ModuleUIDescriptor> moduleUiDescriptors = null)
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

        public ModuleUIDescriptor GetModuleUI()
        {
            return null;
        }

        public IEnumerable<string> GetOptionKeys()
        {
            return null;
        }
    }
}
