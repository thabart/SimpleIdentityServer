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

using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Repositories;

namespace SimpleIdentityServer.DataAccess.SqlServer
{
    public static class SimpleIdentityServerSqlServerExtensions
    {
        public static IServiceCollection AddSimpleIdentityServerSqlServer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ITranslationRepository, TranslationRepository>();
            serviceCollection.AddTransient<IResourceOwnerRepository, ResourceOwnerRepository>();
            serviceCollection.AddTransient<IScopeRepository, ScopeRepository>();
            serviceCollection.AddTransient<IAuthorizationCodeRepository, AuthorizationCodeRepository>();
            serviceCollection.AddTransient<IClientRepository, ClientRepository>();
            serviceCollection.AddTransient<IConsentRepository, ConsentRepository>();
            serviceCollection.AddTransient<IGrantedTokenRepository, GrantedTokenRepository>();
            serviceCollection.AddTransient<IJsonWebKeyRepository, JsonWebKeyRepository>();
            return serviceCollection;
        }
    }
}
