using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.EventStore.EF;
using System;

namespace SimpleIdentityServer.EF.SqlServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStoreSqlServerEF(this IServiceCollection serviceCollection, string connectionString, Action<SqlServerDbContextOptionsBuilder> callback)
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
            serviceCollection.AddEntityFrameworkSqlServer()
                .AddDbContext<EventStoreContext>(options =>
                    options.UseSqlServer(connectionString, callback));
            return serviceCollection;
        }
    }
}
