using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.EventStore.EF;
using SimpleIdentityServer.EventStore.Host.Builders;
using SimpleIdentityServer.EventStore.Host.Configurations;
using SimpleIdentityServer.EventStore.Host.Parsers;
using System;

namespace SimpleIdentityServer.EventStore.Host.Extensions
{
    public static class ApplicationBuilderExtensions
    {

        public static IServiceCollection AddEventStore(this IServiceCollection services, EventStoreConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // 1. Add the dependencies.
            RegisterServices(services, configuration);
            // 2. Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // 3. Add authentication.
            services.AddAuthentication();
            // 4. Add the dependencies needed to run ASP.NET API.
            services.AddMvc();
            return services;
        }

        private static void RegisterServices(IServiceCollection services, EventStoreConfiguration configuration)
        {
            services.AddTransient<ISearchParameterParser, SearchParameterParser>();
            services.AddTransient<IHalLinkBuilder, HalLinkBuilder>();
            switch (configuration.DataSource.Type)
            {
                case DbTypes.INMEMORY:
                    services.AddEventStoreInMemory();
                    break;
                case DbTypes.SQLITE:
                    services.AddEventStoreSqlLite(configuration.DataSource.ConnectionString);
                    break;
                case DbTypes.POSTGRES:
                    services.AddEventStorePostgre(configuration.DataSource.ConnectionString);
                    break;
                case DbTypes.SQLSERVER:
                    services.AddEventStoreSqlServer(configuration.DataSource.ConnectionString);
                    break;
            }
        }

        private static void RegisterDependencies(IServiceCollection services)
        {
            services.AddTransient<ISearchParameterParser, SearchParameterParser>();
            services.AddTransient<IHalLinkBuilder, HalLinkBuilder>();
        }
    }
}
