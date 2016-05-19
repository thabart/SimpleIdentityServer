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
using SimpleIdentityServer.UmaManager.Client.Operation;
using System;

namespace SimpleIdentityServer.UmaManager.Client
{
    public interface IIdentityServerUmaManagerClientFactory
    {
        IOperationClient GetOperationClient();
    }

    public class IdentityServerUmaManagerClientFactory : IIdentityServerUmaManagerClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        #region Constructor

        public IdentityServerUmaManagerClientFactory()
        {
            var services = new ServiceCollection();
            RegisterDependencies(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        #endregion

        #region Public methods

        public IOperationClient GetOperationClient()
        {
            var operationClient = (IOperationClient)_serviceProvider.GetService(typeof(IOperationClient));
            return operationClient;
        }

        #endregion

        #region Private static methods

        private static void RegisterDependencies(IServiceCollection serviceCollection)
        {
            // Register clients
            serviceCollection.AddTransient<IOperationClient, OperationClient>();

            // Register operations
            serviceCollection.AddTransient<ISearchOperationsAction, SearchOperationsAction>();
        }

        #endregion
    }
}
