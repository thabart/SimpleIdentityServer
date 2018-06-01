using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.EventStore.Host.Builders;
using SimpleIdentityServer.EventStore.Host.Parsers;
using System;

namespace SimpleIdentityServer.EventStore.Host.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStoreHost(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            RegisterServices(services);
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
