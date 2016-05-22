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

using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.DataAccess.Fake;
using SimpleIdentityServer.DataAccess.Fake.Extensions;
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.Host.Configuration;
using SimpleIdentityServer.Host.Controllers;
using SimpleIdentityServer.Host.Parsers;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.RateLimitation;
using Swashbuckle.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleIdentityServer.Host 
{    
    public enum DataSourceTypes 
    {
        InMemory,
        SqlServer,
        SqlLite
    }
    
    public sealed class DataSourceOptions
    {        
        /// <summary>
        /// Choose the type of your DataSource
        /// </summary>
        public DataSourceTypes DataSourceType { get; set;}
        
         /// <summary>
        /// Connection string
        /// </summary>
        public string ConnectionString { get; set;}
        
        /// <summary>
        /// List of fake clients
        /// </summary>
        public List<Client> Clients { get; set; }
        
        /// <summary>
        /// List of fake Json Web Keys
        /// </summary>
        public List<JsonWebKey> JsonWebKeys { get; set;}
        
        /// <summary>
        /// List of fake resource owners
        /// </summary>
        public List<ResourceOwner> ResourceOwners { get; set;}
        
        /// <summary>
        /// List of fake scopes
        /// </summary>
        public List<Scope> Scopes { get; set; }
        
        /// <summary>
        /// List of fake translations
        /// </summary>
        public List<Translation> Translations { get; set; }
    }
    
    
    public static class ServiceCollectionExtensions 
    {
        #region Public static methods
        
        public static void AddSimpleIdentityServer(
            this IServiceCollection serviceCollection,
            Action<DataSourceOptions> dataSourceCallback,
            Action<SwaggerOptions> swaggerCallback) 
        {
            if (dataSourceCallback == null)
            {
                throw new ArgumentNullException(nameof(dataSourceCallback));
            }
            
            if (swaggerCallback == null) 
            {
                throw new ArgumentNullException(nameof(swaggerCallback));
            }
            
            var dataSourceOptions = new DataSourceOptions();
            var swaggerOptions = new SwaggerOptions();
            dataSourceCallback(dataSourceOptions);
            swaggerCallback(swaggerOptions);
            serviceCollection.AddSimpleIdentityServer(
                dataSourceOptions,
                swaggerOptions);
        }
        
        public static void AddSimpleIdentityServer(
            this IServiceCollection serviceCollection, 
            DataSourceOptions dataSourceOptions,
            SwaggerOptions swaggerOptions) 
        {
            if (dataSourceOptions == null) {
                throw new ArgumentNullException(nameof(dataSourceOptions));
            }            
            
            if (swaggerOptions == null) {
                throw new ArgumentNullException(nameof(swaggerOptions));
            }
            
            if (dataSourceOptions.DataSourceType == DataSourceTypes.InMemory) 
            {
                serviceCollection.AddSimpleIdentityServerFake();
                var clients = dataSourceOptions.Clients.Select(c => c.ToFake()).ToList();
                var jsonWebKeys = dataSourceOptions.JsonWebKeys.Select(j => j.ToFake()).ToList();
                var resourceOwners = dataSourceOptions.ResourceOwners.Select(r => r.ToFake()).ToList();
                var scopes = dataSourceOptions.Scopes.Select(s => s.ToFake()).ToList();
                var translations = dataSourceOptions.Translations.Select(t => t.ToFake()).ToList();
                var fakeDataSource = new FakeDataSource()
                {
                    Clients = clients,
                    JsonWebKeys = jsonWebKeys,
                    ResourceOwners = resourceOwners,
                    Scopes = scopes,
                    Translations = translations
                };

                serviceCollection.AddTransient(a => fakeDataSource);
            }

            if (dataSourceOptions.DataSourceType == DataSourceTypes.SqlServer)
            {
                serviceCollection.AddSimpleIdentityServerSqlServer(dataSourceOptions.ConnectionString);
            }

            if (dataSourceOptions.DataSourceType == DataSourceTypes.SqlLite)
            {
                serviceCollection.AddSimpleIdentityServerSqlLite(dataSourceOptions.ConnectionString);
            }
            
            ConfigureSimpleIdentityServer(serviceCollection, swaggerOptions);
        }
        
        #endregion
        
        #region Private static methods
        
        /// <summary>
        /// Add all the dependencies needed to run Simple Identity Server
        /// </summary>
        /// <param name="services"></param>
        /// <param name="swaggerOptions"></param>
        private static void ConfigureSimpleIdentityServer(
            IServiceCollection services,
            SwaggerOptions swaggerOptions) 
        {
            services.AddSimpleIdentityServerCore();
            services.AddSimpleIdentityServerJwt();
            services.AddRateLimitation();

            var logging = SimpleIdentityServerEventSource.Log;
            services.AddTransient<ICertificateStore, CertificateStore>();
            services.AddTransient<IResourceOwnerService, InMemoryUserService>();
            services.AddTransient<IRedirectInstructionParser, RedirectInstructionParser>();
            services.AddTransient<IActionResultParser, ActionResultParser>();
            services.AddTransient<ISimpleIdentityServerConfigurator, ConcreteSimpleIdentityServerConfigurator>();
            services.AddInstance<ISimpleIdentityServerEventSource>(logging);
            if (swaggerOptions.IsSwaggerEnabled)
            {
                services.AddSwaggerGen();
                services.ConfigureSwaggerDocument(opts => {
                    opts.SingleApiVersion(new Info
                    {
                        Version = "v1",
                        Title = "Simple Identity Server",
                        TermsOfService = "None"
                    });
                });
            }

            services.AddDataProtection();
            services.AddInstance<SwaggerOptions>(swaggerOptions);
            services.AddAuthentication(opts => opts.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
            services.AddMvc();
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProvider = new EmbeddedFileProvider(
                        typeof(AuthenticateController).GetTypeInfo().Assembly,
                        "SimpleIdentityServer.Host"
                );
            });
        }
        
        #endregion
    }
}