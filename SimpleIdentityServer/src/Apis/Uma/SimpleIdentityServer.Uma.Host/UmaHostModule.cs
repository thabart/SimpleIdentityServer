using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using SimpleIdentityServer.Uma.Host.Configurations;
using SimpleIdentityServer.Uma.Host.Controllers;
using SimpleIdentityServer.Uma.Host.Extensions;
using SimpleIdentityServer.Uma.Host.Middlewares;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Host
{
    public class UmaHostModule : IModule
    {
        public void Configure(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseUmaExceptionHandler(new ExceptionHandlerMiddlewareOptions
            {
                UmaEventSource = applicationBuilder.ApplicationServices.GetService<IUmaServerEventSource>()
            });
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

            services.AddUmaHost(new UmaHostConfiguration());
            var assembly = typeof(ConfigurationController).Assembly;
            mvcBuilder.AddApplicationPart(assembly);
        }

        public IEnumerable<string> GetOptionKeys()
        {
            return null;
        }
    }
}
