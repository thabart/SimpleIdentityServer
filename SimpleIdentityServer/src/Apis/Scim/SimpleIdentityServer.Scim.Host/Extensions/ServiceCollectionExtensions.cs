using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Scim.Core;
using SimpleIdentityServer.Scim.Db.EF;
using SimpleIdentityServer.Scim.Host.Configurations;
using System;
using WebApiContrib.Core.Concurrency;
using WebApiContrib.Core.Storage.InMemory;
using WebApiContrib.Core.Storage;

namespace SimpleIdentityServer.Scim.Host.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScim(this IServiceCollection services, ScimConfiguration configuration)
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
            // 2. Add authorization policies.
            services.AddAuthorization(options =>
            {
                options.AddPolicy("scim_manage", policy => policy.RequireClaim("scope", "scim_manage"));
                options.AddPolicy("scim_read", policy => policy.RequireClaim("scope", "scim_read"));
            });
            // 3. Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // 4. Add authentication.
            services.AddAuthentication();
            // 5. Add the dependencies needed to run ASP.NET API.
            services.AddMvc();
            return services;
        }

        private static void RegisterServices(IServiceCollection services, ScimConfiguration configuration)
        {
            switch(configuration.Caching.Type)
            {
                case CachingTypes.INMEMORY:
                    services.AddConcurrency(opt => opt.UseInMemory());
                    break;
                case CachingTypes.REDIS:
                    services.AddConcurrency(opt => opt.UseRedis(o =>
                    {
                        o.Configuration = configuration.Caching.ConnectionString;
                        o.InstanceName = configuration.Caching.InstanceName;
                    }, configuration.Caching.Port));
                    break;
            }
            services.AddScim();
            switch(configuration.DataSource.Type)
            {
                case DbTypes.INMEMORY:
                    services.AddInMemoryDb();
                    break;
                case DbTypes.POSTGRES:
                    services.AddScimPostgresql(configuration.DataSource.ConnectionString);
                    break;
                case DbTypes.SQLSERVER:
                    services.AddScimSqlServer(configuration.DataSource.ConnectionString);
                    break;
            }
        }
    }
}
