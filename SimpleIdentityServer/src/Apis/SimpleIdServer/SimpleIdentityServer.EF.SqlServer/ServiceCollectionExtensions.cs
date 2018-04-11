using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.EF.SqlServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOAuthSqlServerEF(this IServiceCollection serviceCollection, string connectionString)
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
            serviceCollection.AddEntityFrameworkSqlServer()
                .AddDbContext<SimpleIdentityServerContext>(options =>
                    options.UseSqlServer(connectionString));
            return serviceCollection;
        }
    }
}
