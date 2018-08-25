using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Scim.Mapping.Ad.Client.Configuration;
using SimpleIdentityServer.Scim.Mapping.Ad.Client.Mapping;
using System;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client
{
    public interface IAdMappingClientFactory
    {
        IAdMappingClient GetAdMappingClient();
        IAdConfigurationClient GetAdConfigurationClient();
    }

    public class AdMappingClientFactory : IAdMappingClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AdMappingClientFactory()
        {
            var services = new ServiceCollection();
            RegisterDependencies(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        public AdMappingClientFactory(IHttpClientFactory httpClientFactory)
        {
            var services = new ServiceCollection();
            RegisterDependencies(services, httpClientFactory);
            _serviceProvider = services.BuildServiceProvider();
        }
        
        public IAdMappingClient GetAdMappingClient()
        {
            var result = (IAdMappingClient)_serviceProvider.GetService(typeof(IAdMappingClient));
            return result;
        }

        public IAdConfigurationClient GetAdConfigurationClient()
        {
            var result = (IAdConfigurationClient)_serviceProvider.GetService(typeof(IAdConfigurationClient));
            return result;
        }

        private static void RegisterDependencies(IServiceCollection serviceCollection, IHttpClientFactory httpClientFactory = null)
        {
            if (httpClientFactory != null)
            {
                serviceCollection.AddSingleton(httpClientFactory);
            }
            else
            {
                serviceCollection.AddCommonClient();
            }

            serviceCollection.AddTransient<IAddAdMappingOperation, AddAdMappingOperation>();
            serviceCollection.AddTransient<IDeleteAdMappingOperation, DeleteAdMappingOperation>();
            serviceCollection.AddTransient<IGetAdMappingOperation, GetAdMappingOperation>();
            serviceCollection.AddTransient<IGetAllAdMappingsOperation, GetAllAdMappingsOperation>();
            serviceCollection.AddTransient<IAdMappingClient, AdMappingClient>();
            serviceCollection.AddTransient<IAdConfigurationClient, AdConfigurationClient>();
            serviceCollection.AddTransient<IGetAdConfigurationOperation, GetAdConfigurationOperation>();
            serviceCollection.AddTransient<IUpdateAdConfigurationOperation, UpdateAdConfigurationOperation>();
            serviceCollection.AddTransient<IGetAdPropertiesOperation, GetAdPropertiesOperation>();
        }
    }
}
