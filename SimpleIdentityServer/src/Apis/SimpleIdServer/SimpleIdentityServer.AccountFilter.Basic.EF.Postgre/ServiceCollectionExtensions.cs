using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.AccountFilter.Basic.EF.Postgre
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBasicAccountFilterPostgresqlEF(this IServiceCollection serviceCollection, string connectionString, Action<NpgsqlDbContextOptionsBuilder> callback = null)
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
            serviceCollection.AddEntityFrameworkNpgsql()
                .AddDbContext<AccountFilterBasicServerContext>(options =>
                    options.UseNpgsql(connectionString, callback));
            return serviceCollection;
        }
    }
}
