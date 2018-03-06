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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.EF.Repositories;
using System;

namespace SimpleIdentityServer.Uma.EF
{
    public static class SimpleIdServerUmaExtensions
    {
        public static IServiceCollection AddSimpleIdServerUmaSqlServer(
            this IServiceCollection serviceCollection,
            string connectionString)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFramework()
                .AddDbContext<SimpleIdServerUmaContext>(options => options.UseSqlServer(connectionString));
            return serviceCollection;
        }

        public static IServiceCollection AddSimpleIdServerUmaPostgresql(
            this IServiceCollection serviceCollection,
            string connectionString)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFramework()
                            .AddDbContext<SimpleIdServerUmaContext>(options => options.UseNpgsql(connectionString));
            return serviceCollection;
        }

        public static IServiceCollection AddSimpleIdServerUmaInMemory(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFramework()
                            .AddDbContext<SimpleIdServerUmaContext>(options => options.UseInMemoryDatabase().ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            return serviceCollection;
        }
        
        private static void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IResourceSetRepository, ResourceSetRepository>();
            serviceCollection.AddTransient<IPolicyRepository, PolicyRepository>();
        }
    }
}
