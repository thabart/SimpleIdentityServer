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
using SimpleIdentityServer.Client.Builders;
using SimpleIdentityServer.Client.Factories;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Selectors;
using System;

namespace SimpleIdentityServer.Client
{
    public interface IIdentityServerClientFactory
    {
        IDiscoveryClient CreateDiscoveryClient();

        IClientAuthSelector CreateTokenClient();

        IJwksClient CreateJwksClient();
    }

    public class IdentityServerClientFactory : IIdentityServerClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        #region Constructor

        public IdentityServerClientFactory()
        {
            var services = new ServiceCollection();
            RegisterDependencies(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        #endregion

        /// <summary>
        /// Get the discovery client
        /// </summary>
        /// <returns>Discovery client</returns>
        public IDiscoveryClient CreateDiscoveryClient()
        {
            var result = (IDiscoveryClient)_serviceProvider.GetService(typeof(IDiscoveryClient));
            return result;
        }

        /// <summary>
        /// Create token client
        /// </summary>
        /// <returns>Choose your client authentication method</returns>
        public IClientAuthSelector CreateTokenClient()
        {
            var result = (IClientAuthSelector)_serviceProvider.GetService(typeof(IClientAuthSelector));
            return result;
        }

        /// <summary>
        /// Create token client
        /// </summary>
        /// <returns>Jwks client</returns>
        public IJwksClient CreateJwksClient()
        {
            var result = (IJwksClient)_serviceProvider.GetService(typeof(IJwksClient));
            return result;
        }

        /// <summary>
        /// Create user information client
        /// </summary>
        /// <returns></returns>
        public IUserInfoClient CreateUserInfoClient()
        {
            var result = (IUserInfoClient)_serviceProvider.GetService(typeof(IUserInfoClient));
            return result;
        }

        /// <summary>
        /// Creates a registration client.
        /// </summary>
        /// <returns></returns>
        public IRegistrationClient CreateRegistrationClient()
        {
            var result = (IRegistrationClient)_serviceProvider.GetService(typeof(IRegistrationClient));
            return result;
        }

        #region Private static methods

        private static void RegisterDependencies(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IHttpClientFactory, HttpClientFactory>();

            // Register clients
            serviceCollection.AddTransient<ITokenClient, TokenClient>();
            serviceCollection.AddTransient<IDiscoveryClient, DiscoveryClient>();
            serviceCollection.AddTransient<IJwksClient, JwksClient>();
            serviceCollection.AddTransient<IUserInfoClient, UserInfoClient>();
            serviceCollection.AddTransient<IRegistrationClient, RegistrationClient>();

            // Register operations
            serviceCollection.AddTransient<IGetDiscoveryOperation, GetDiscoveryOperation>();
            serviceCollection.AddTransient<IPostTokenOperation, PostTokenOperation>();
            serviceCollection.AddTransient<IGetJsonWebKeysOperation, GetJsonWebKeysOperation>();
            serviceCollection.AddTransient<IRegisterClientOperation, RegisterClientOperation>();

            // Register request builders
            serviceCollection.AddScoped<ITokenRequestBuilder, TokenRequestBuilder>();

            // Register selectors
            serviceCollection.AddTransient<IClientAuthSelector, ClientAuthSelector>();
            serviceCollection.AddTransient<ITokenGrantTypeSelector, TokenGrantTypeSelector>();
        }

        #endregion
    }
}
