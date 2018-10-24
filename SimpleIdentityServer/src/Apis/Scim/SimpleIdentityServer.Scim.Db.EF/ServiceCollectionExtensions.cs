using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Scim.Core.Stores;
using SimpleIdentityServer.Scim.Db.EF.Stores;

namespace SimpleIdentityServer.Scim.Db.EF
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimRepository(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ISchemaStore, SchemaStore>();
            serviceCollection.AddTransient<IRepresentationStore, RepresentationStore>();
            return serviceCollection;
        }
    }
}
