using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using System;

namespace SimpleIdentityServer.Scim.Db.EF.Postgre
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimPostgresqlEF(this IServiceCollection serviceCollection, string connectionString, Action<NpgsqlDbContextOptionsBuilder> callback = null)
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
            serviceCollection.AddEntityFrameworkNpgsql()
                .AddDbContext<ScimDbContext>(options =>
                    options.UseNpgsql(connectionString, callback));
            return serviceCollection;
        }
    }
}
