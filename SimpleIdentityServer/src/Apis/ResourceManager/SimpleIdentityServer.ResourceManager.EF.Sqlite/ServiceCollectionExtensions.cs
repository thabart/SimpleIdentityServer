using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.ResourceManager.EF.Sqlite
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddResourceManagerSqliteEF(this IServiceCollection serviceCollection, string connectionString)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            serviceCollection.AddResourceManagerRepositories();
            serviceCollection.AddEntityFrameworkSqlite()
                .AddDbContext<ResourceManagerDbContext>(options =>
                    options.UseSqlite(connectionString));
            return serviceCollection;
        }
    }
}
