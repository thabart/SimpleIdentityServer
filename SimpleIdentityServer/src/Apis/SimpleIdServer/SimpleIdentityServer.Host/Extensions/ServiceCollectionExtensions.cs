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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Host.Configuration;
using SimpleIdentityServer.Host.Parsers;
using SimpleIdentityServer.Host.Services;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.OAuth.Logging;
using SimpleIdentityServer.OpenId.Logging;
using System;

namespace SimpleIdentityServer.Host
{
    public static class ServiceCollectionExtensions 
    {
        public static IServiceCollection AddOpenIdApi(
            this IServiceCollection serviceCollection,
            Action<IdentityServerOptions> optionsCallback)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (optionsCallback == null)
            {
                throw new ArgumentNullException(nameof(optionsCallback));
            }
            
            var options = new IdentityServerOptions();
            optionsCallback(options);
            serviceCollection.AddOpenIdApi(
                options);
            return serviceCollection;
        }
        
        /// <summary>
        /// Add the OPENID API.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddOpenIdApi(
            this IServiceCollection serviceCollection,
            IdentityServerOptions options)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            
            ConfigureSimpleIdentityServer(
                serviceCollection, 
                options);
            return serviceCollection;
        }

        public static AuthorizationOptions AddOpenIdSecurityPolicy(this AuthorizationOptions authenticateOptions, string cookieName)
        {
            if (authenticateOptions == null)
            {
                throw new ArgumentNullException(nameof(authenticateOptions));
            }

            authenticateOptions.AddPolicy("Connected", policy => policy.RequireAssertion((ctx) => {
                return ctx.User.Identity != null && ctx.User.Identity.AuthenticationType == cookieName;
            }));
            return authenticateOptions;
        }

        private static void ConfigureSimpleIdentityServer(
            IServiceCollection services,
            IdentityServerOptions options)
        {
            services.AddSimpleIdentityServerCore()
                .AddSimpleIdentityServerJwt()
                .AddHostIdentityServer(options)
                .AddIdServerClient()
                .AddTechnicalLogging()
                .AddOpenidLogging()
                .AddOAuthLogging()
                .AddDataProtection();
        }

        public static IServiceCollection AddHostIdentityServer(this IServiceCollection serviceCollection, IdentityServerOptions options)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.AuthenticateResourceOwner == null)
            {
                serviceCollection.AddTransient<IAuthenticateResourceOwnerService, DefaultAuthenticateResourceOwerService>();
            }
            else
            {
                serviceCollection.AddTransient(typeof(IAuthenticateResourceOwnerService), options.AuthenticateResourceOwner);
            }

            if (options.ConfigurationService == null)
            {
                serviceCollection.AddTransient<IConfigurationService, DefaultConfigurationService>();
            }
            else
            {
                serviceCollection.AddTransient(typeof(IConfigurationService), options.ConfigurationService);
            }

            if (options.PasswordService == null)
            {
                serviceCollection.AddTransient<IPasswordService, DefaultPasswordService>();
            }
            else
            {
                serviceCollection.AddTransient(typeof(IPasswordService), options.PasswordService);
            }
                        
            serviceCollection
                .AddSingleton(options.Authenticate)
                .AddSingleton(options.Scim)
                .AddTransient<IRedirectInstructionParser, RedirectInstructionParser>()
                .AddTransient<IActionResultParser, ActionResultParser>()
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                .AddDataProtection();
            return serviceCollection;
        }
    }
}