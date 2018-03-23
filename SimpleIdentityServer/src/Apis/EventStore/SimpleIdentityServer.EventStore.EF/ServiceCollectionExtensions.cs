#region copyright
// Copyright 2017 Habart Thierry
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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.EventStore.Core.Repositories;
using SimpleIdentityServer.EventStore.EF.Parsers;
using SimpleIdentityServer.EventStore.EF.Repositories;
using System;

namespace SimpleIdentityServer.EventStore.EF
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStoreSqlServer(this IServiceCollection serviceCollection, string connectionString)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFrameworkSqlServer()
                .AddDbContext<EventStoreContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Transient);
            return serviceCollection;
        }

        public static IServiceCollection AddEventStoreSqlLite(this IServiceCollection serviceCollection, string connectionString)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFrameworkSqlite()
                .AddDbContext<EventStoreContext>(options => options.UseSqlite(connectionString), ServiceLifetime.Transient);
            return serviceCollection;
        }

        public static IServiceCollection AddEventStorePostgre(this IServiceCollection serviceCollection, string connectionString)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFrameworkNpgsql()
                .AddDbContext<EventStoreContext>(options =>
                    options.UseNpgsql(connectionString), ServiceLifetime.Transient);
            return serviceCollection;
        }

        public static IServiceCollection AddEventStoreInMemory(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<EventStoreContext>((options => options.UseInMemoryDatabase().ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))), ServiceLifetime.Transient);
            return serviceCollection;
        }


        private static void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IEventAggregateRepository, EventAggregateRepository>();
            serviceCollection.AddTransient<IFilterParser, FilterParser>();
        }
    }
}
