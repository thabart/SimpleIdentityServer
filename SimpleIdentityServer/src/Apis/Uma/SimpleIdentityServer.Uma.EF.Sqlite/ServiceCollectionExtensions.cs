using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.Uma.EF.Sqlite
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUmaSqlite(this IServiceCollection serviceCollection, string connectionString)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            serviceCollection.AddUmaRepositories();
            serviceCollection.AddEntityFrameworkSqlite()
                .AddDbContext<SimpleIdServerUmaContext>(options =>
                    options.UseSqlite(connectionString));
            return serviceCollection;
        }
    }
}
