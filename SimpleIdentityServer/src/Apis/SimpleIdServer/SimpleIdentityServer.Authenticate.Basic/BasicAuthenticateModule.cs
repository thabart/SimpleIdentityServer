using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using System;

namespace SimpleIdentityServer.Authenticate.Basic
{
    public class BasicAuthenticateModule : IModule
    {
        public void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder = null, IHostingEnvironment env = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            services.AddBasicAuthentication(mvcBuilder, env);
        }

        public void Configure(IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            applicationBuilder.UseMvc(routes =>
            {
                routes.UseUserPasswordAuthentication();
            });
        }
    }
}
