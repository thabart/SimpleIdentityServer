using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.AccountFilter.Basic.EF.Repositories;
using SimpleIdentityServer.AccountFilter.Basic.Repositories;
using System;

namespace SimpleIdentityServer.AccountFilter.Basic.EF
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAccountFilterRepositories(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddTransient<IFilterRepository, FilterRepository>();
            return serviceCollection;
        }
    }
}
