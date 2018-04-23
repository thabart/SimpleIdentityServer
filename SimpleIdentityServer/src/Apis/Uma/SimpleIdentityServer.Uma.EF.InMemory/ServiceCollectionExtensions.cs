using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.Uma.EF.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUmaInMemoryEF(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddUmaRepositories();
            serviceCollection.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<SimpleIdServerUmaContext>(options => options.UseInMemoryDatabase().ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            return serviceCollection;
        }
    }
}
