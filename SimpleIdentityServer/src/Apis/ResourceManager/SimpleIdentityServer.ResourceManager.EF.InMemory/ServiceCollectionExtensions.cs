using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.ResourceManager.EF.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddResourceManagerInMemoryEF(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddResourceManagerRepositories();
            serviceCollection.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ResourceManagerDbContext>(options => options.UseInMemoryDatabase().ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            return serviceCollection;
        }
    }
}
