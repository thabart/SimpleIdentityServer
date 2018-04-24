using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.EventStore.EF;
using System;

namespace SimpleIdentityServer.EF.Sqlite
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStoreSqliteEF(this IServiceCollection serviceCollection, string connectionString, Action<SqliteDbContextOptionsBuilder> callback)
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
                    options.UseSqlite(connectionString, callback));
            return serviceCollection;
        }
    }
}
