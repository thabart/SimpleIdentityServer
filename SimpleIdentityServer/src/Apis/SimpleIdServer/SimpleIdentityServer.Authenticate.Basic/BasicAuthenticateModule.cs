using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Authenticate.Basic
{
    public class BasicAuthenticateModule : IModule
    {
        private const string ClientId = "ClientId";
        private const string ClientSecret = "ClientSecret";
        private const string AuthorizationWellKnownConfiguration = "AuthorizationWellKnownConfiguration";
        private const string BaseScimUrl = "BaseScimUrl";
        private const string IsScimResourceAutomaticallyCreated = "IsScimResourceAutomaticallyCreated";
        private const string ClaimsIncludedInUserCreation = "ClaimsIncludedInUserCreation";

        public static ModuleUIDescriptor ModuleUi = new ModuleUIDescriptor
        {
            Title = "Authenticate",
            RelativeUrl = "~/Authenticate",
            IsAuthenticated = false,
            Picture = "~/img/Unknown.png"
        };

        public void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder = null, IHostingEnvironment env = null, IDictionary<string, string> options = null, IEnumerable<ModuleUIDescriptor> moduleUIDescriptors = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            var opts = GetOptions(options);
            services.AddBasicAuthentication(mvcBuilder, env, opts);
        }

        public void Configure(IApplicationBuilder applicationBuilder)
        {
        }

        public void Configure(IRouteBuilder routeBuilder)
        {
            if (routeBuilder == null)
            {
                throw new ArgumentNullException(nameof(routeBuilder));
            }

            routeBuilder.UseUserPasswordAuthentication();
        }

        public void ConfigureAuthorization(AuthorizationOptions authorizationOptions, IDictionary<string, string> options = null)
        {
        }

        public void ConfigureAuthentication(AuthenticationBuilder authBuilder, IDictionary<string, string> options = null)
        {
        }

        public IEnumerable<string> GetOptionKeys()
        {
            return new[]
            {
                ClientId,
                ClientSecret,
                AuthorizationWellKnownConfiguration,
                BaseScimUrl,
                IsScimResourceAutomaticallyCreated,
                ClaimsIncludedInUserCreation
            };
        }

        public ModuleUIDescriptor GetModuleUI()
        {
            return ModuleUi;
        }

        private static BasicAuthenticateOptions GetOptions(IDictionary<string, string> options)
        {
            var result = new BasicAuthenticateOptions();
            if (options == null)
            {
                return result;
            }

            result.AuthenticationOptions = new BasicAuthenticationOptions
            {
                AuthorizationWellKnownConfiguration = options.TryGetValue(AuthorizationWellKnownConfiguration),
                ClientId = options.TryGetValue(ClientId),
                ClientSecret = options.TryGetValue(ClientSecret)
            };
            result.ScimBaseUrl = options.TryGetValue(BaseScimUrl);
            result.ClaimsIncludedInUserCreation = options.TryGetArr(ClaimsIncludedInUserCreation);
            bool isScimResourceAutomaticallyCreated;
            if (options.TryGetValue(IsScimResourceAutomaticallyCreated, out isScimResourceAutomaticallyCreated))
            {
                result.IsScimResourceAutomaticallyCreated = isScimResourceAutomaticallyCreated;
            }

            return result;
        }
    }
}
