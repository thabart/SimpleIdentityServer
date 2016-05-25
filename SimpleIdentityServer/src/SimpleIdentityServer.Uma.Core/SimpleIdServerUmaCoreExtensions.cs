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
using SimpleIdentityServer.Uma.Core.Api.Authorization;
using SimpleIdentityServer.Uma.Core.Api.Authorization.Actions;
using SimpleIdentityServer.Uma.Core.Api.CodeSampleController;
using SimpleIdentityServer.Uma.Core.Api.CodeSampleController.Actions;
using SimpleIdentityServer.Uma.Core.Api.ConfigurationController;
using SimpleIdentityServer.Uma.Core.Api.ConfigurationController.Actions;
using SimpleIdentityServer.Uma.Core.Api.IntrospectionController;
using SimpleIdentityServer.Uma.Core.Api.IntrospectionController.Actions;
using SimpleIdentityServer.Uma.Core.Api.PermissionController;
using SimpleIdentityServer.Uma.Core.Api.PermissionController.Actions;
using SimpleIdentityServer.Uma.Core.Api.PolicyController;
using SimpleIdentityServer.Uma.Core.Api.PolicyController.Actions;
using SimpleIdentityServer.Uma.Core.Api.ResourceSetController;
using SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions;
using SimpleIdentityServer.Uma.Core.Api.ScopeController;
using SimpleIdentityServer.Uma.Core.Api.ScopeController.Actions;
using SimpleIdentityServer.Uma.Core.Code;
using SimpleIdentityServer.Uma.Core.Helpers;
using SimpleIdentityServer.Uma.Core.Policies;
using SimpleIdentityServer.Uma.Core.Validators;
using System;

namespace SimpleIdentityServer.Uma.Core
{
    public static class SimpleIdServerUmaCoreExtensions
    {
        #region Public static methods

        public static IServiceCollection AddSimpleIdServerUmaCore(
            this IServiceCollection serviceCollection,
            UmaServerOptions umaServerOptions)
        {
            if (umaServerOptions == null)
            {
                throw new ArgumentNullException(nameof(umaServerOptions));
            }

            RegisterDependencies(serviceCollection, umaServerOptions);
            return serviceCollection;
        }

        public static IServiceCollection AddSimpleIdServerUmaCore(
            this IServiceCollection serviceCollection,
            Action<UmaServerOptions> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var umaServerOptions = new UmaServerOptions();
            callback(umaServerOptions);
            RegisterDependencies(serviceCollection, umaServerOptions);
            return serviceCollection;
        }

        #endregion

        #region Private static methods

        private static void RegisterDependencies(
            IServiceCollection serviceCollection,
            UmaServerOptions umaServerOptions)
        {
            serviceCollection.AddTransient<IResourceSetActions, ResourceSetActions>();
            serviceCollection.AddTransient<IAddResourceSetAction, AddResourceSetAction>();
            serviceCollection.AddTransient<IGetResourceSetAction, GetResourceSetAction>();
            serviceCollection.AddTransient<IUpdateResourceSetAction, UpdateResourceSetAction>();
            serviceCollection.AddTransient<IDeleteResourceSetAction, DeleteResourceSetAction>();
            serviceCollection.AddTransient<IGetAllResourceSetAction, GetAllResourceSetAction>();
            serviceCollection.AddTransient<IResourceSetParameterValidator, ResourceSetParameterValidator>();
            serviceCollection.AddTransient<IGetScopeAction, GetScopeAction>();
            serviceCollection.AddTransient<IScopeActions, ScopeActions>();
            serviceCollection.AddTransient<IScopeParameterValidator, ScopeParameterValidator>();
            serviceCollection.AddTransient<IInsertScopeAction, InsertScopeAction>();
            serviceCollection.AddTransient<IScopeActions, ScopeActions>();
            serviceCollection.AddTransient<IUpdateScopeAction, UpdateScopeAction>();
            serviceCollection.AddTransient<IDeleteScopeAction, DeleteScopeAction>();
            serviceCollection.AddTransient<IGetScopesAction, GetScopesAction>();
            serviceCollection.AddTransient<IPermissionControllerActions, PermissionControllerActions>();
            serviceCollection.AddTransient<IAddPermissionAction, AddPermissionAction>();
            serviceCollection.AddTransient<IRepositoryExceptionHelper, RepositoryExceptionHelper>();
            serviceCollection.AddTransient<IGetAuthorizationAction, GetAuthorizationAction>();
            serviceCollection.AddTransient<IAuthorizationPolicyValidator, AuthorizationPolicyValidator>();
            serviceCollection.AddTransient<IBasicAuthorizationPolicy, BasicAuthorizationPolicy>();
            serviceCollection.AddTransient<ICustomAuthorizationPolicy, CustomAuthorizationPolicy>();
            serviceCollection.AddTransient<IAuthorizationActions, AuthorizationActions>();
            serviceCollection.AddTransient<IAddAuthorizationPolicyAction, AddAuthorizationPolicyAction>();
            serviceCollection.AddTransient<IPolicyActions, PolicyActions>();
            serviceCollection.AddTransient<IGetAuthorizationPolicyAction, GetAuthorizationPolicyAction>();
            serviceCollection.AddTransient<IDeleteAuthorizationPolicyAction, DeleteAuthorizationPolicyAction>();
            serviceCollection.AddTransient<IGetAuthorizationPoliciesAction, GetAuthorizationPoliciesAction>();
            serviceCollection.AddTransient<IUpdatePolicyAction, UpdatePolicyAction>();
            serviceCollection.AddTransient<IConfigurationActions, ConfigurationActions>();
            serviceCollection.AddTransient<IGetConfigurationAction, GetConfigurationAction>();
            serviceCollection.AddTransient<IGetIntrospectAction, GetIntrospectAction>();
            serviceCollection.AddTransient<IIntrospectionActions, IntrospectionActions>();
            serviceCollection.AddTransient<IGetBackendCodeAction, GetBackendCodeAction>();
            serviceCollection.AddTransient<ICodeProvider, CodeProvider>();
            serviceCollection.AddTransient<ICodeSampleActions, CodeSampleActions>();
            serviceCollection.AddTransient<IGetFrontendCodeAction, GetFrontendCodeAction>();
            serviceCollection.AddSingleton(umaServerOptions);
        }

        #endregion
    }
}
