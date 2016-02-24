using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using SimpleIdentityServer.Host.Configuration;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.DataAccess.Fake;
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.DataAccess.Fake.Extensions;
using SimpleIdentityServer.Host.Parsers;
using SimpleIdentityServer.Logging;
using Swashbuckle.SwaggerGen;
using SimpleIdentityServer.RateLimitation;
using Microsoft.AspNet.Authentication.Cookies;
using System.Collections.Generic;
using SimpleIdentityServer.Core.Models;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.FileProviders;
using SimpleIdentityServer.Host.Controllers;
using System.Reflection;

namespace SimpleIdentityServer.Host 
{    
    public enum DataSourceTypes 
    {
        InMemory,
        SqlServer
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
                serviceCollection.AddTransient<FakeDataSource>(a => fakeDataSource);
            }
            else 
            {
                serviceCollection.AddSimpleIdentityServerSqlServer();
                // serviceCollection.AddTransient<SimpleIdentityServerContext>((a) => new SimpleIdentityServerContext(dataSourceOptions.ConnectionString));
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
            services.AddTransient<ICacheManager, CacheManager>();
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

            services.AddInstance<SwaggerOptions>(swaggerOptions);
            services.AddAuthentication(opts => opts.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
            services.AddMvc();
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProvider = new CompositeFileProvider(
                    new EmbeddedFileProvider(
                        typeof(AuthenticateController).GetTypeInfo().Assembly,
                        "SimpleIdentityServer.Host"
                    ),
                    options.FileProvider
                );
            });
        }
        
        #endregion
    }
}