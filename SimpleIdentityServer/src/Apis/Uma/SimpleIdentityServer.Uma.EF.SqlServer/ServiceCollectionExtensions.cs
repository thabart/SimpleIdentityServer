using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.Uma.EF.SqlServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUmaSqlServerEF(this IServiceCollection serviceCollection, string connectionString, Action<SqlServerDbContextOptionsBuilder> callback = null)
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
            serviceCollection.AddEntityFrameworkSqlServer()
                .AddDbContext<SimpleIdServerUmaContext>(options =>
                    options.UseSqlServer(connectionString, callback));
            return serviceCollection;
        }
    }
}
