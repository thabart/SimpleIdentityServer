using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.AccountFilter.Basic.EF.Sqlite
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBasicAccountFilterSqliteEF(this IServiceCollection serviceCollection, string connectionString, Action<SqliteDbContextOptionsBuilder> callback = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            serviceCollection.AddAccountFilterRepositories();
            serviceCollection.AddEntityFrameworkSqlite()
                .AddDbContext<AccountFilterBasicServerContext>(options =>
                    options.UseSqlite(connectionString, callback));
            return serviceCollection;
        }
    }
}
