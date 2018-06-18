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
        private const string IsExternalAccountAutomaticallyCreated = "IsExternalAccountAutomaticallyCreated";

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
                IsExternalAccountAutomaticallyCreated
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

            bool b = false;
            var str = TryGetStr(options, IsExternalAccountAutomaticallyCreated);
            bool.TryParse(str, out b);
            result.AuthenticationOptions = new BasicAuthenticationOptions
            {
                AuthorizationWellKnownConfiguration = TryGetStr(options, AuthorizationWellKnownConfiguration),
                ClientId = TryGetStr(options, ClientId),
                ClientSecret = TryGetStr(options, ClientSecret)
            };
            result.ScimBaseUrl = TryGetStr(options, BaseScimUrl);
            result.IsExternalAccountAutomaticallyCreated = b;
            return result;
        }

        private static string TryGetStr(IDictionary<string, string> opts, string name)
        {
            if (opts.ContainsKey(name))
            {
                return opts[name];
            }

            return null;
        }
    }
}
