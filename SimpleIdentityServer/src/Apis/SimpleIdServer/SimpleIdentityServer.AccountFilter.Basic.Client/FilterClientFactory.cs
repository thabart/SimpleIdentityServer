using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.AccountFilter.Basic.Client.Operations;
using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Common.Client.Factories;
using System;

namespace SimpleIdentityServer.AccountFilter.Basic.Client
{
    public interface IFilterClientFactory
    {
        IFilterClient GetFilterClient();
    }

    public class FilterClientFactory : IFilterClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public FilterClientFactory()
        {
            var services = new ServiceCollection();
            RegisterDependencies(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        public FilterClientFactory(IHttpClientFactory httpClientFactory)
        {
            var services = new ServiceCollection();
            RegisterDependencies(services, httpClientFactory);
            _serviceProvider = services.BuildServiceProvider();
        }

        public IFilterClient GetFilterClient()
        {
            var result = (IFilterClient)_serviceProvider.GetService(typeof(IFilterClient));
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

            serviceCollection.AddTransient<IAddFilterOperation, AddFilterOperation>();
            serviceCollection.AddTransient<IDeleteFilterOperation, DeleteFilterOperation>();
            serviceCollection.AddTransient<IGetAllFiltersOperation, GetAllFiltersOperation>();
            serviceCollection.AddTransient<IGetFilterOperation, GetFilterOperation>();
            serviceCollection.AddTransient<IUpdateFilterOperation, UpdateFilterOperation>();
            serviceCollection.AddTransient<IFilterClient, FilterClient>();
        }
    }
}
