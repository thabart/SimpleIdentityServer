using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.ResourceManager.Core.Repositories;
using SimpleIdentityServer.ResourceManager.EF.Repositories;

namespace SimpleIdentityServer.ResourceManager.EF
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddResourceManagerSqlServer(this IServiceCollection serviceCollection, string connectionString)
        {
            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFrameworkSqlServer()
                .AddDbContext<ResourceManagerDbContext>(options =>
                    options.UseSqlServer(connectionString));
            return serviceCollection;
        }

        public static IServiceCollection AddResourceManagerSqlLite(
            this IServiceCollection serviceCollection,
            string connectionString)
        {
            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFrameworkSqlite()
                .AddDbContext<ResourceManagerDbContext>(options =>
                    options.UseSqlite(connectionString));
            return serviceCollection;
        }

        public static IServiceCollection AddResourceManagerPostgre(
            this IServiceCollection serviceCollection,
            string connectionString)
        {
            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFrameworkNpgsql()
                .AddDbContext<ResourceManagerDbContext>(options =>
                    options.UseNpgsql(connectionString));
            return serviceCollection;
        }

        public static IServiceCollection AddResourceManagerInMemory(
            this IServiceCollection serviceCollection)
        {
            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ResourceManagerDbContext>(options => options.UseInMemoryDatabase().ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            return serviceCollection;
        }

        private static void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IAssetRepository, AssetRepository>();
            serviceCollection.AddTransient<IIdProviderRepository, IdProviderRepository>();
        }
    }
}
