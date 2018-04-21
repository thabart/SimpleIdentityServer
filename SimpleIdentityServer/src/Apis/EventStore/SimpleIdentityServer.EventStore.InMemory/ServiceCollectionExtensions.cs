using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using SimpleIdentityServer.EventStore.EF;

namespace SimpleIdentityServer.EventStore.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStoreInMemoryEF(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddEventStoreRepositories();
            serviceCollection.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<EventStoreContext>(options => options.UseInMemoryDatabase().ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            return serviceCollection;
        }
    }
}
