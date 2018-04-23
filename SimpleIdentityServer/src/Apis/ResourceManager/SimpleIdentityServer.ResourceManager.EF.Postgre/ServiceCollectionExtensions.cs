using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.ResourceManager.EF.Postgre
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddResourceManagerPostgreEF(this IServiceCollection serviceCollection, string connectionString)
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
            serviceCollection.AddEntityFrameworkNpgsql()
                .AddDbContext<ResourceManagerDbContext>(options =>
                    options.UseNpgsql(connectionString));
            return serviceCollection;
        }
    }
}
