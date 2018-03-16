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

using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Configuration.Core.Repositories;
using SimpleIdentityServer.Configuration.IdServer.EF.Repositories;

namespace SimpleIdentityServer.Configuration.IdServer.EF
{
    public static class IdServerConfigurationEfExtensions
    {
        public static IServiceCollection AddIdServerConfigurationSqlServer(
            this IServiceCollection serviceCollection,
            string configurationConnectionString,
            string idServerConnectionString)
        {
            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFramework()
                .AddDbContext<IdServerConfigurationDbContext>(options =>
                    options.UseSqlServer(configurationConnectionString));
            serviceCollection.AddEntityFramework()
                .AddDbContext<ConfigurationDbContext>(options =>
                    options.UseSqlServer(idServerConnectionString));
            return serviceCollection;
        }

        public static IServiceCollection AddIdServerConfigurationPostgre(
            this IServiceCollection serviceCollection,
            string configurationConnectionString,
            string idServerConnectionString)
        {
            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFramework()
                .AddDbContext<IdServerConfigurationDbContext>(options =>
                    options.UseNpgsql(configurationConnectionString));
            serviceCollection.AddEntityFramework()
                .AddDbContext<ConfigurationDbContext>(options =>
                    options.UseNpgsql(idServerConnectionString));
            return serviceCollection;
        }


        public static IServiceCollection AddIdServerConfigurationInMemory(this IServiceCollection serviceCollection)
        {
            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFramework()
                .AddDbContext<IdServerConfigurationDbContext>(options =>
                    options.UseInMemoryDatabase());
            serviceCollection.AddEntityFramework()
                .AddDbContext<ConfigurationDbContext>(options =>
                    options.UseInMemoryDatabase());
            return serviceCollection;
        }

        private static void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IAuthenticationProviderRepository, AuthenticationProviderRepository>();
            serviceCollection.AddTransient<ISettingRepository, SettingRepository>();
        }
    }
}
