using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.Scim.Db.EF.SqlServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimSqlServerEF(this IServiceCollection serviceCollection, string connectionString, Action<SqlServerDbContextOptionsBuilder> callback = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            
			serviceCollection.AddScimRepository();
            serviceCollection.AddEntityFrameworkSqlServer()
                .AddDbContext<ScimDbContext>(options =>
                    options.UseSqlServer(connectionString, callback));
            return serviceCollection;
        }
    }
}
