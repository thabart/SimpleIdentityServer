#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Manager.Client.Configuration;
using SimpleIdentityServer.Manager.Client.Clients;
using SimpleIdentityServer.Manager.Client.Factories;

namespace SimpleIdentityServer.Manager.Client
{
    public interface IOpenIdManagerClientFactory
    {
        IConfigurationClient GetConfigurationClient();
        IOpenIdClients GetOpenIdsClient();
    }

    internal sealed class OpenIdManagerClientFactory : IOpenIdManagerClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public OpenIdManagerClientFactory()
        {
            var services = new ServiceCollection();
            RegisterDependencies(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        public IConfigurationClient GetConfigurationClient()
        {
            var authProviderClient = (IConfigurationClient)_serviceProvider.GetService(typeof(IConfigurationClient));
            return authProviderClient;
        }

        public IOpenIdClients GetOpenIdsClient()
        {
            var configurationClient = (IOpenIdClients)_serviceProvider.GetService(typeof(IOpenIdClients));
            return configurationClient;
        }

        private static void RegisterDependencies(IServiceCollection serviceCollection)
        {
            // Register clients
            serviceCollection.AddTransient<IOpenIdClients, OpenIdClients>();
            serviceCollection.AddTransient<IConfigurationClient, ConfigurationClient>();
            
            // Regsiter factories
            serviceCollection.AddTransient<IHttpClientFactory, HttpClientFactory>();

            // Register operations
            serviceCollection.AddTransient<IGetAllClientsOperation, GetAllClientsOperation>();
            serviceCollection.AddTransient<IGetConfigurationOperation, GetConfigurationOperation>();
            serviceCollection.AddTransient<IGetClientOperation, GetClientOperation>();
            serviceCollection.AddTransient<IDeleteClientOperation, DeleteClientOperation>();
            serviceCollection.AddTransient<ISearchClientOperation, SearchClientOperation>();
            serviceCollection.AddTransient<IUpdateClientOperation, UpdateClientOperation>();
            serviceCollection.AddTransient<IAddClientOperation, AddClientOperation>();
        }
    }
}
