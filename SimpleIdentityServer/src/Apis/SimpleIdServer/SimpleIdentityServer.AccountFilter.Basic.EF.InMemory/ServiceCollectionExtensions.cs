using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.AccountFilter.Basic.EF.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBasicAccountFilterInMemoryEF(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddAccountFilterRepositories();
            serviceCollection.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<AccountFilterBasicServerContext>(options => options.UseInMemoryDatabase().ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            return serviceCollection;
        }
    }
}
