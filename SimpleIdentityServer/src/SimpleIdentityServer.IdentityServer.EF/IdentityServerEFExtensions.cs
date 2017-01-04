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
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Services;
using IdentityServer4.EntityFramework.Stores;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.IdentityServer.EF.DbContexts;
using SimpleIdentityServer.IdentityServer.EF.Repositories;
using System;

namespace SimpleIdentityServer.IdentityServer.EF
{
    public static class IdentityServerEFExtensions
    {
        public static IServiceCollection AddSimpleIdentityServerSqlServer(
            this IServiceCollection serviceCollection,
            string connectionString,
            string assembly)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFramework()
                .AddDbContext<ConfigurationDbContext>(options =>
                    options.UseSqlServer(connectionString, opts => opts.MigrationsAssembly(assembly)));
            serviceCollection.AddEntityFramework()
                .AddDbContext<PersistedGrantDbContext>(options =>
                    options.UseSqlServer(connectionString, opts => opts.MigrationsAssembly(assembly)));
            serviceCollection.AddEntityFramework()
                .AddDbContext<UserDbContext>(options =>
                    options.UseSqlServer(connectionString, opts => opts.MigrationsAssembly(assembly)));
            return serviceCollection;
        }

        public static IServiceCollection AddSimpleIdentityServerPostGre(
            this IServiceCollection serviceCollection,
            string connectionString,
            string assembly)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFramework()
                .AddDbContext<ConfigurationDbContext>(options =>
                    options.UseNpgsql(connectionString, opts => opts.MigrationsAssembly(assembly)));
            serviceCollection.AddEntityFramework()
                .AddDbContext<PersistedGrantDbContext>(options =>
                    options.UseNpgsql(connectionString, opts => opts.MigrationsAssembly(assembly)));
            serviceCollection.AddEntityFramework()
                .AddDbContext<UserDbContext>(options =>
                    options.UseNpgsql(connectionString, opts => opts.MigrationsAssembly(assembly)));
            return serviceCollection;
        }

        public static IServiceCollection AddSimpleIdentityServerInMemory(
            this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFramework()
                .AddDbContext<ConfigurationDbContext>(opts =>
                    opts.UseInMemoryDatabase().ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            serviceCollection.AddEntityFramework()
                .AddDbContext<PersistedGrantDbContext>(opts =>
                    opts.UseInMemoryDatabase().ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            serviceCollection.AddEntityFramework()
                .AddDbContext<UserDbContext>(opts =>
                    opts.UseInMemoryDatabase().ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            return serviceCollection;
        }

        private static void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IConfigurationDbContext, ConfigurationDbContext>();
            serviceCollection.AddScoped<IPersistedGrantDbContext, PersistedGrantDbContext>();
            serviceCollection.AddTransient<IClientStore, ClientStore>();
            serviceCollection.AddTransient<IScopeStore, ScopeStore>();
            serviceCollection.AddTransient<IPersistedGrantStore, PersistedGrantStore>();
            serviceCollection.AddTransient<ICorsPolicyService, CorsPolicyService>();
            serviceCollection.AddTransient<IScopeRepository, ScopeRepository>();
            serviceCollection.AddTransient<IResourceOwnerRepository, ResourceOwnerRepository>();
            serviceCollection.AddTransient<IClientRepository, ClientRepository>();
        }
    }
}
