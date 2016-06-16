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
using SimpleIdentityServer.Configuration.Core.Api.AuthProvider;
using SimpleIdentityServer.Configuration.Core.Api.AuthProvider.Actions;

namespace SimpleIdentityServer.Configuration.Core
{
    public static class SimpleIdentityServerConfigurationExtensions
    {
        #region Public static methods

        public static IServiceCollection AddSimpleIdentityServerConfiguration(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IAuthProviderActions, AuthProviderActions>();
            serviceCollection.AddTransient<IGetAuthenticationProvider, GetAuthenticationProvider>();
            serviceCollection.AddTransient<IGetAuthenticationProviders, GetAuthenticationProviders>();
            serviceCollection.AddTransient<IActivateAuthenticationProvider, ActivateAuthenticationProvider>();
            return serviceCollection;
        }

        #endregion
    }
}
