using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using System;

namespace SimpleIdentityServer.EF.Postgre
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOAuthPostgresqlEF(this IServiceCollection serviceCollection, string connectionString, Action<NpgsqlDbContextOptionsBuilder> callback = null)
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
            serviceCollection.AddEntityFrameworkNpgsql().AddDbContext<SimpleIdentityServerContext>(options => options.UseNpgsql(connectionString, callback));
            return serviceCollection;
        }
    }
}
