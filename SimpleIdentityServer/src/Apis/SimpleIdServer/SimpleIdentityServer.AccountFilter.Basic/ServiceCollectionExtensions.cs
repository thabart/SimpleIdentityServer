using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.AccountFilter.Basic.Controllers;
using System;

namespace SimpleIdentityServer.AccountFilter.Basic
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAccountFilter(this IServiceCollection services, IMvcBuilder mvcBuilder)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            var assembly = typeof(FiltersController).Assembly;
            mvcBuilder.AddApplicationPart(assembly);
            services.AddTransient<IAccountFilter, AccountFilter>();
            return services;
        }
    }
}
