using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.Uma.EF.SqlServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUmaSqlServerEF(this IServiceCollection serviceCollection, string connectionString)
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
                    options.UseSqlServer(connectionString));
            return serviceCollection;
        }
    }
}
