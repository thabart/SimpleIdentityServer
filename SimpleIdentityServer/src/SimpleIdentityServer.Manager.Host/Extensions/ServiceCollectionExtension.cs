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
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.Manager.Core;
using System;
using System.Reflection;

namespace SimpleIdentityServer.Manager.Host.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddSimpleIdentityServerManager(
            this IServiceCollection serviceCollection,
            AuthorizationServerOptions authorizationServerOptions,
            DatabaseOptions databaseOptions,
            SwaggerOptions swaggerOptions)
        {
            if (authorizationServerOptions == null)
            {
                throw new ArgumentNullException(nameof(authorizationServerOptions));
            }

            if (databaseOptions == null)
            {
                throw new ArgumentNullException(nameof(databaseOptions));
            }

            if (swaggerOptions == null)
            {
                throw new ArgumentNullException(nameof(swaggerOptions));
            }

            // Add the dependencies needed to enable CORS
            serviceCollection.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));


            // Enable SQLServer
            if (databaseOptions.DataSourceType == DataSourceTypes.SqlServer)
            {
                serviceCollection.AddSimpleIdentityServerSqlServer(databaseOptions.ConnectionString);
            }

            // Enable SQLite
            if (databaseOptions.DataSourceType == DataSourceTypes.SqlLite)
            {
                serviceCollection.AddSimpleIdentityServerSqlLite(databaseOptions.ConnectionString);
            }

            // Enable postgresql
            if (databaseOptions.DataSourceType == DataSourceTypes.Postgres)
            {
                serviceCollection.AddSimpleIdentityServerPostgre(databaseOptions.ConnectionString);
            }

            serviceCollection.AddSimpleIdentityServerCore();
            serviceCollection.AddSimpleIdentityServerManagerCore();

            // Add authentication
            serviceCollection.AddAuthentication();

            // Add authorization policy rules
            serviceCollection.AddAuthorization(options =>
            {
                options.AddPolicy("manager", policy => policy.RequireClaim("scope", "openid_manager"));
            });

            serviceCollection.AddSimpleIdentityServerJwt();

            // Add the dependencies needed to run MVC
            serviceCollection.AddMvc();
        }
    }
}
