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

using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.Manager.Core;
using SimpleIdentityServer.Manager.Host.Hal;
using SimpleIdentityServer.Manager.Host.Swagger;
using Swashbuckle.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimpleIdentityServer.Manager.Host.Extensions
{
    public static class ServiceCollectionExtension
    {
        private class AssignOauth2SecurityRequirements : IOperationFilter
        {
            public void Apply(Operation operation, OperationFilterContext context)
            {
                var assignedScopes = new List<string>
                {
                    "openid",
                    "SimpleIdentityServerManager:GetClients"
                };

                var oauthRequirements = new Dictionary<string, IEnumerable<string>>
                {
                    {
                        "oauth2", assignedScopes
                    }
                };

                operation.Security = new List<IDictionary<string, IEnumerable<string>>>();
                operation.Security.Add(oauthRequirements);
            }
        }

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

            // Add the dependencies needed to run Swagger
            serviceCollection.AddSwaggerGen();
            serviceCollection.ConfigureSwaggerDocument(opts => {
                opts.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Simple identity server manager",
                    TermsOfService = "None"
                });
                opts.SecurityDefinitions.Add("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = authorizationServerOptions.AuthorizationUrl,
                    TokenUrl = authorizationServerOptions.TokenUrl,
                    Description = "Implicit flow",
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "OpenId" },
                        { "role" , "Get the roles" }
                    }
                });
                opts.OperationFilter<AssignOauth2SecurityRequirements>();
            });

            // Add the dependencies needed to enable CORS
            serviceCollection.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));


            // Enable SqlServer
            serviceCollection.AddSimpleIdentityServerSqlServer(databaseOptions.ConnectionString);
            serviceCollection.AddSimpleIdentityServerCore();
            serviceCollection.AddSimpleIdentityServerManagerCore();

            // Add authentication
            serviceCollection.AddAuthentication();

            // Add authorization policy rules
            serviceCollection.AddAuthorization(options =>
            {
                options.AddPolicy("getAllClients", policy => policy.RequireClaim("role", "administrator"));
                options.AddPolicy("getClient", policy => policy.RequireClaim("role", "administrator"));
                options.AddPolicy("deleteClient", policy => policy.RequireClaim("role", "administrator"));
                options.AddPolicy("updateClient", policy => policy.RequireClaim("role", "administrator"));
            });

            serviceCollection.AddSimpleIdentityServerJwt();

            // Add the dependencies needed to run MVC
            serviceCollection.AddMvc(options =>
            {
                options.OutputFormatters.Add(new JsonHalMediaTypeFormatter());
            });

            if (swaggerOptions.IsEnabled)
            {
                serviceCollection.Configure<RazorViewEngineOptions>(options =>
                {
                    options.FileProvider = new CompositeFileProvider(
                        new EmbeddedFileProvider(
                            typeof(SwaggerUiController).GetTypeInfo().Assembly,
                            "SimpleIdentityServer.Manager.Host"
                        ),
                        options.FileProvider
                    );
                });
            }
        }
    }
}
