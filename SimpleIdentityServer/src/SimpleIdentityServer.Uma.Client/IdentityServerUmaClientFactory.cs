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

using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Client.Authorization;
using SimpleIdentityServer.Client.Configuration;
using SimpleIdentityServer.Client.Factory;
using SimpleIdentityServer.Client.Permission;
using SimpleIdentityServer.Client.ResourceSet;
using System;

namespace SimpleIdentityServer.Client
{
    public interface IIdentityServerUmaClientFactory
    {
        IPermissionClient GetPermissionClient();

        IResourceSetClient GetResourceSetClient();

        IAuthorizationClient GetAuthorizationClient();
    }

    public class IdentityServerUmaClientFactory : IIdentityServerUmaClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        #region Constructor

        public IdentityServerUmaClientFactory()
        {
            var services = new ServiceCollection();
            RegisterDependencies(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        #endregion

        #region Public methods

        public IPermissionClient GetPermissionClient()
        {
            var permissionClient = (IPermissionClient)_serviceProvider.GetService(typeof(IPermissionClient));
            return permissionClient;
        }

        public IResourceSetClient GetResourceSetClient()
        {
            var resourceSetClient = (IResourceSetClient)_serviceProvider.GetService(typeof(IResourceSetClient));
            return resourceSetClient;
        }

        public IAuthorizationClient GetAuthorizationClient()
        {
            var authorizationClient = (IAuthorizationClient)_serviceProvider.GetService(typeof(IAuthorizationClient));
            return authorizationClient;
        }

        #endregion

        #region Private static methods

        private static void RegisterDependencies(IServiceCollection serviceCollection)
        {
            // Register clients
            serviceCollection.AddTransient<IResourceSetClient, ResourceSetClient>();
            serviceCollection.AddTransient<IPermissionClient, PermissionClient>();
            serviceCollection.AddTransient<IAuthorizationClient, AuthorizationClient>();

            // Regsiter factories
            serviceCollection.AddTransient<IHttpClientFactory, HttpClientFactory>();

            // Register operations
            serviceCollection.AddTransient<IAddPermissionOperation, AddPermissionOperation>();
            serviceCollection.AddTransient<IGetConfigurationOperation, GetConfigurationOperation>();
            serviceCollection.AddTransient<IAddResourceSetOperation, AddResourceSetOperation>();
            serviceCollection.AddTransient<IDeleteResourceSetOperation, DeleteResourceSetOperation>();
            serviceCollection.AddTransient<IGetAuthorizationOperation, GetAuthorizationOperation>();
        }

        #endregion
    }
}
