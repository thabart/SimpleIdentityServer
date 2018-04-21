using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.EventStore.EF;
using System;

namespace SimpleIdentityServer.EF.Sqlite
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStoreSqliteEF(this IServiceCollection serviceCollection, string connectionString)
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
            serviceCollection.AddEntityFrameworkSqlite()
                .AddDbContext<EventStoreContext>(options =>
                    options.UseSqlite(connectionString));
            return serviceCollection;
        }
    }
}
