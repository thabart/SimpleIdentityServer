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

using Microsoft.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.EF.Repositories;
using System;

namespace SimpleIdentityServer.Uma.EF
{
    public static class SimpleIdServerUmaExtensions
    {
        #region Public static methods

        public static IServiceCollection AddSimpleIdServerUmaSqlServer(
            this IServiceCollection serviceCollection,
            string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            RegisterServices(serviceCollection);
            serviceCollection.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<SimpleIdServerUmaContext>(options => options.UseSqlServer(connectionString));
            return serviceCollection;
        }

        #endregion

        #region Private static methods

        private static void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IResourceSetRepository, ResourceSetRepository>();
            serviceCollection.AddTransient<IScopeRepository, ScopeRepository>();
            serviceCollection.AddTransient<ITicketRepository, TicketRepository>();
        }

        #endregion
    }
}
