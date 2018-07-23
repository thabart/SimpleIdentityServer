using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Common.Client.Factories;
using System;

namespace SimpleIdentityServer.Common.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommonClient(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddTransient<IHttpClientFactory, HttpClientFactory>();
            return serviceCollection;
        }
    }
}
