using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.ResourceManager.Core.Repositories;
using SimpleIdentityServer.ResourceManager.EF.Repositories;
using System;

namespace SimpleIdentityServer.ResourceManager.EF
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddResourceManagerRepositories(this IServiceCollection serviceCollection)
        {
			if (serviceCollection == null) 
			{
				throw new ArgumentNullException(nameof(serviceCollection));
			}
			
            serviceCollection.AddTransient<IAssetRepository, AssetRepository>();
            serviceCollection.AddTransient<IEndpointRepository, EndpointRepository>();
			return serviceCollection;
        }
    }
}
