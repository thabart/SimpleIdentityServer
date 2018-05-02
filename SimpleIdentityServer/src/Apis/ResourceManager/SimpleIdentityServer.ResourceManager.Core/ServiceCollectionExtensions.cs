using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.ResourceManager.Core.Api.Clients;
using SimpleIdentityServer.ResourceManager.Core.Api.Clients.Actions;
using SimpleIdentityServer.ResourceManager.Core.Api.Profile;
using SimpleIdentityServer.ResourceManager.Core.Api.Profile.Actions;
using SimpleIdentityServer.ResourceManager.Core.Api.ResourceOwners;
using SimpleIdentityServer.ResourceManager.Core.Api.ResourceOwners.Actions;
using SimpleIdentityServer.ResourceManager.Core.Api.Resources;
using SimpleIdentityServer.ResourceManager.Core.Api.Resources.Actions;
using SimpleIdentityServer.ResourceManager.Core.Api.Scopes;
using SimpleIdentityServer.ResourceManager.Core.Api.Scopes.Actions;
using SimpleIdentityServer.ResourceManager.Core.Helpers;
using SimpleIdentityServer.ResourceManager.Core.Stores;
using SimpleIdentityServer.Uma.Client;
using System;

namespace SimpleIdentityServer.ResourceManager.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddResourceManager(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddIdServerClient();
            services.AddUmaClient();
            services.AddOpenIdManagerClient();
            services.AddTransient<IEndpointHelper, EndpointHelper>();
            services.AddTransient<ISearchResourcesetAction, SearchResourcesetAction>();
            services.AddTransient<IResourcesetActions, ResourcesetActions>();
            services.AddTransient<IGetAuthPoliciesByResourceAction, GetAuthPoliciesByResourceAction>();
            services.AddTransient<IGetResourceAction, GetResourceAction>();
            services.AddTransient<IProfileActions, ProfileActions>();
            services.AddTransient<IGetProfileAction, GetProfileAction>();
            services.AddTransient<IUpdateProfileAction, UpdateProfileAction>();
            services.AddTransient<IClientActions, ClientActions>();
            services.AddTransient<IAddClientAction, AddClientAction>();
            services.AddTransient<IDeleteClientAction, DeleteClientAction>();
            services.AddTransient<IGetClientAction, GetClientAction>();
            services.AddTransient<ISearchClientsAction, SearchClientsAction>();
            services.AddTransient<IUpdateClientAction, UpdateClientAction>();
            services.AddTransient<IScopeActions, ScopeActions>();
            services.AddTransient<IAddScopeAction, AddScopeAction>();
            services.AddTransient<IDeleteScopeAction, DeleteScopeAction>();
            services.AddTransient<IGetScopeAction, GetScopeAction>();
            services.AddTransient<ISearchScopesAction, SearchScopesAction>();
            services.AddTransient<IUpdateScopeAction, UpdateScopeAction>();
            services.AddTransient<IRequestHelper, RequestHelper>();
            services.AddTransient<IResourceOwnerActions, ResourceOwnerActions>();
            services.AddTransient<IAddResourceOwnerAction, AddResourceOwnerAction>();
            services.AddTransient<IDeleteResourceOwnerAction, DeleteResourceOwnerAction>();
            services.AddTransient<IGetResourceOwnerAction, GetResourceOwnerAction>();
            services.AddTransient<ISearchResourceOwnersAction, SearchResourceOwnersAction>();
            services.AddTransient<IUpdateResourceOwnerAction, UpdateResourceOwnerAction>();
            return services;
        }

        public static IServiceCollection AddInMemoryTokenStore(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<ITokenStore, InMemoryTokenStore>();
            return services;
        }
    }
}
