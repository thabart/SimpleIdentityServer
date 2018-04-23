using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.Uma.EF.Postgre
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUmaPostgreEF(this IServiceCollection serviceCollection, string connectionString)
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
            serviceCollection.AddEntityFrameworkNpgsql()
                .AddDbContext<SimpleIdServerUmaContext>(options =>
                    options.UseNpgsql(connectionString));
            return serviceCollection;
        }
    }
}
