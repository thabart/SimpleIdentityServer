using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.Scim.Db.EF.SqlServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimSqlServerEF(this IServiceCollection serviceCollection, string connectionString)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            serviceCollection.AddScimRepositories();
            serviceCollection.AddEntityFrameworkSqlServer()
                .AddDbContext<ScimDbContext>(options =>
                    options.UseSqlServer(connectionString));
            return serviceCollection;
        }
    }
}
