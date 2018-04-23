using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.ResourceManager.EF.SqlServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddResourceManagerSqlServerEF(this IServiceCollection serviceCollection, string connectionString)
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
            serviceCollection.AddEntityFrameworkSqlServer()
                .AddDbContext<ResourceManagerDbContext>(options =>
                    options.UseSqlServer(connectionString));
            return serviceCollection;
        }
    }
}
