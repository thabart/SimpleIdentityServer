using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace WebApiContrib.Core.Storage.InMemory
{
    public class InMemoryStorageModule : IModule
    {
        public void Configure(IApplicationBuilder applicationBuilder)
        {
        }

        public void Configure(IRouteBuilder routeBuilder)
        {
        }

        public void ConfigureAuthentication(AuthenticationBuilder authBuilder, IDictionary<string, string> options = null)
        {
        }

        public void ConfigureAuthorization(AuthorizationOptions authorizationOptions, IDictionary<string, string> options = null)
        {
        }

        public void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder = null, IHostingEnvironment env = null, IDictionary<string, string> options = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var storage = new InMemoryStorage();
            var storageOptions = new StorageOptions
            {
                Storage = storage
            };
            services.AddSingleton(storageOptions);
            services.AddSingleton<IStorage>(storage);
        }

        public IEnumerable<string> GetOptionKeys()
        {
            return new string[0];
        }
    }
}
