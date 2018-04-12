using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.EventStore.Host.Builders;
using SimpleIdentityServer.EventStore.Host.Parsers;
using System;

namespace SimpleIdentityServer.EventStore.Host.Extensions
{
    public static class ApplicationBuilderExtensions
    {

        public static IServiceCollection AddEventStore(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // 1. Add the dependencies.
            RegisterServices(services);
            // 2. Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // 3. Add the dependencies needed to run ASP.NET API.
            services.AddMvc();
            return services;
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<ISearchParameterParser, SearchParameterParser>();
            services.AddTransient<IHalLinkBuilder, HalLinkBuilder>();
        }

        private static void RegisterDependencies(IServiceCollection services)
        {
            services.AddTransient<ISearchParameterParser, SearchParameterParser>();
            services.AddTransient<IHalLinkBuilder, HalLinkBuilder>();
        }
    }
}
