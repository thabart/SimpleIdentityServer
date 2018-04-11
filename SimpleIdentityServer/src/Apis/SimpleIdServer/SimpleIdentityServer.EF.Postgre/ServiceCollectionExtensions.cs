using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.EF.Postgre
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOAuthPostgresqlEF(this IServiceCollection serviceCollection, string connectionString)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            serviceCollection.AddOAuthRepositories();
            serviceCollection.AddEntityFrameworkNpgsql()
                .AddDbContext<SimpleIdentityServerContext>(options =>
                    options.UseNpgsql(connectionString));
            return serviceCollection;
        }
    }
}
