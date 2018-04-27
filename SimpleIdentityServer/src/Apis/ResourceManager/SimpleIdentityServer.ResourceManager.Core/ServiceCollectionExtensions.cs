using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.ResourceManager.Core.Api.Profile;
using SimpleIdentityServer.ResourceManager.Core.Api.Profile.Actions;
using SimpleIdentityServer.ResourceManager.Core.Api.Resources;
using SimpleIdentityServer.ResourceManager.Core.Api.Resources.Actions;
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
            services.AddTransient<IEndpointHelper, EndpointHelper>();
            services.AddTransient<ISearchResourcesetAction, SearchResourcesetAction>();
            services.AddTransient<IResourcesetActions, ResourcesetActions>();
            services.AddTransient<IGetAuthPoliciesByResourceAction, GetAuthPoliciesByResourceAction>();
            services.AddTransient<IGetResourceAction, GetResourceAction>();
            services.AddTransient<IProfileActions, ProfileActions>();
            services.AddTransient<IGetProfileAction, GetProfileAction>();
            services.AddTransient<IUpdateProfileAction, UpdateProfileAction>();
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
