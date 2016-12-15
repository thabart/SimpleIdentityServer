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
using SimpleIdentityServer.Client.Introspection;
using SimpleIdentityServer.Client.Permission;
using SimpleIdentityServer.Client.Policy;
using SimpleIdentityServer.Client.ResourceSet;
using SimpleIdentityServer.Client.Scope;
using SimpleIdentityServer.Uma.Client.Factory;
using System;

namespace SimpleIdentityServer.Client
{
    public interface IIdentityServerUmaClientFactory
    {
        IPermissionClient GetPermissionClient();
        IResourceSetClient GetResourceSetClient();
        IAuthorizationClient GetAuthorizationClient();
        IPolicyClient GetPolicyClient();
        IIntrospectionClient GetIntrospectionClient();
        IScopeClient GetScopeClient();
    }

    public class IdentityServerUmaClientFactory : IIdentityServerUmaClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public IdentityServerUmaClientFactory()
        {
            var services = new ServiceCollection();
            RegisterDependencies(services);
            _serviceProvider = services.BuildServiceProvider();
        }

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

        public IPolicyClient GetPolicyClient()
        {
            var policyClient = (IPolicyClient)_serviceProvider.GetService(typeof(IPolicyClient));
            return policyClient;
        }

        public IIntrospectionClient GetIntrospectionClient()
        {
            var introspectionClient = (IIntrospectionClient)_serviceProvider.GetService(typeof(IIntrospectionClient));
            return introspectionClient;
        }

        public IScopeClient GetScopeClient()
        {
            var scopeClient = (IScopeClient)_serviceProvider.GetService(typeof(IScopeClient));
            return scopeClient;
        }

        private static void RegisterDependencies(IServiceCollection serviceCollection)
        {
            // Register clients
            serviceCollection.AddTransient<IResourceSetClient, ResourceSetClient>();
            serviceCollection.AddTransient<IPermissionClient, PermissionClient>();
            serviceCollection.AddTransient<IAuthorizationClient, AuthorizationClient>();
            serviceCollection.AddTransient<IPolicyClient, PolicyClient>();
            serviceCollection.AddTransient<IIntrospectionClient, IntrospectionClient>();
            serviceCollection.AddTransient<IScopeClient, ScopeClient>();

            // Regsiter factories
            serviceCollection.AddTransient<IHttpClientFactory, HttpClientFactory>();

            // Register operations
            serviceCollection.AddTransient<IAddPermissionOperation, AddPermissionOperation>();
            serviceCollection.AddTransient<IGetConfigurationOperation, GetConfigurationOperation>();
            serviceCollection.AddTransient<IAddResourceSetOperation, AddResourceSetOperation>();
            serviceCollection.AddTransient<IDeleteResourceSetOperation, DeleteResourceSetOperation>();
            serviceCollection.AddTransient<IGetAuthorizationOperation, GetAuthorizationOperation>();
            serviceCollection.AddTransient<IAddPolicyOperation, AddPolicyOperation>();
            serviceCollection.AddTransient<IGetPolicyOperation, GetPolicyOperation>();
            serviceCollection.AddTransient<IDeletePolicyOperation, DeletePolicyOperation>();
            serviceCollection.AddTransient<IGetPoliciesOperation, GetPoliciesOperation>();
            serviceCollection.AddTransient<IGetIntrospectionAction, GetIntrospectionAction>();
            serviceCollection.AddTransient<IGetResourcesOperation, GetResourcesOperation>();
            serviceCollection.AddTransient<IGetResourceOperation, GetResourceOperation>();
            serviceCollection.AddTransient<IUpdateResourceOperation, UpdateResourceOperation>();
            serviceCollection.AddTransient<IAddResourceToPolicyOperation, AddResourceToPolicyOperation>();
            serviceCollection.AddTransient<IDeleteResourceFromPolicyOperation, DeleteResourceFromPolicyOperation>();
            serviceCollection.AddTransient<IUpdatePolicyOperation, UpdatePolicyOperation>();
            serviceCollection.AddTransient<IGetScopeOperation, GetScopeOperation>();
            serviceCollection.AddTransient<IGetScopesOperation, GetScopesOperation>();
            serviceCollection.AddTransient<IDeleteScopeOperation, DeleteScopeOperation>();
            serviceCollection.AddTransient<IUpdateScopeOperation, UpdateScopeOperation>();
            serviceCollection.AddTransient<IAddScopeOperation, AddScopeOperation>();

        }
    }
}
