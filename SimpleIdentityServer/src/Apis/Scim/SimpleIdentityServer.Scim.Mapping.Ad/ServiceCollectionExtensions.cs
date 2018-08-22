using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Scim.Mapping.Ad.Stores;
using System;

namespace SimpleIdentityServer.Scim.Mapping.Ad
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimMapping(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<IAttributeMapper, AttributeMapper>();
            services.AddTransient<IMappingStore, MappingStore>();
            services.AddTransient<IConfigurationStore, ConfigurationStore>();
            services.AddTransient<IUserFilterParser, UserFilterParser>();
            return services;
        }
    }
}
