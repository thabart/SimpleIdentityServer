using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.EventStore.EF;
using System;

namespace SimpleIdentityServer.EF.Postgre
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStorePostgresqlEF(this IServiceCollection serviceCollection, string connectionString)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            serviceCollection.AddEventStoreRepositories();
            serviceCollection.AddEntityFrameworkNpgsql()
                .AddDbContext<EventStoreContext>(options =>
                    options.UseNpgsql(connectionString));
            return serviceCollection;
        }
    }
}
