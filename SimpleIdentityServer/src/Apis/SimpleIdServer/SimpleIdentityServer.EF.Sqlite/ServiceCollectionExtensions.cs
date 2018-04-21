using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.EF.Sqlite
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOAuthSqliteEF(this IServiceCollection serviceCollection, string connectionString)
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
            serviceCollection.AddEntityFrameworkSqlite()
                .AddDbContext<SimpleIdentityServerContext>(options =>
                    options.UseSqlite(connectionString));
            return serviceCollection;
        }
    }
}
