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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SimpleIdentityServer.Api.Controllers.Api;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Api.Jwks.Actions;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.Host.Tests.Extensions;
using SimpleIdentityServer.Host.Tests.Services;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WebApiContrib.Core.Storage;
using WebApiContrib.Core.Storage.InMemory;

namespace SimpleIdentityServer.Host.Tests
{
    public class FakeStartup : IStartup
    {
        public const string ScimEndPoint = "http://localhost:5555/";
        private IdentityServerOptions _options;
        private IJsonWebKeyEnricher _jsonWebKeyEnricher;
        private SharedContext _context;

        public FakeStartup(SharedContext context)
        {
            _options = new IdentityServerOptions
            {

                IsDeveloperModeEnabled = false,
                DataSource = new DataSourceOptions
                {
                    DataSourceType = DataSourceTypes.InMemory,
                    IsDataMigrated = false
                },
                Logging = new LoggingOptions
                {
                    ElasticsearchOptions = new ElasticsearchOptions
                    {
                        IsEnabled = false
                    },
                    FileLogOptions = new FileLogOptions
                    {
                        IsEnabled = false
                    }
                },
                Authenticate = new AuthenticateOptions
                {
                    CookieName = "SimpleIdServer"
                },
                Scim = new ScimOptions
                {
                    IsEnabled = true,
                    EndPoint = ScimEndPoint
                },
                AuthenticateResourceOwner = typeof(CustomAuthenticateResourceOwnerService)
            };
            _jsonWebKeyEnricher = new JsonWebKeyEnricher();
            _context = context;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // 1. Configure the caching
            services.AddStorage(opt => opt.UseInMemory());
            // 2. Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // 3. Configure Simple identity server
            ConfigureIdServer(services);
            // 4. Configure MVC
            var mvc = services.AddMvc();
            var parts = mvc.PartManager.ApplicationParts;
            parts.Clear();
            parts.Add(new AssemblyPart(typeof(DiscoveryController).GetTypeInfo().Assembly));
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<SimpleIdentityServerContext>();
                context.EnsureSeedData(_context);
            }

            //1 . Enable CORS.
            app.UseCors("AllowAll");
            // 2. Use static files.
            app.UseStaticFiles();
            // 3. Use simple identity server.
            app.UseSimpleIdentityServer(_options);
            // 4. Client JWKS endpoint
            app.Map("/jwks_client", a =>
            {
                a.Run(async ctx =>
                {
                    var jwks = new[]
                    {
                        _context.EncryptionKey,
                        _context.SignatureKey
                    };
                    var jsonWebKeySet = new JsonWebKeySet();
                    var publicKeysUsedToValidateSignature = ExtractPublicKeysForSignature(jwks);
                    var publicKeysUsedForClientEncryption = ExtractPrivateKeysForSignature(jwks);
                    var result = new JsonWebKeySet
                    {
                        Keys = new List<Dictionary<string, object>>()
                    };

                    result.Keys.AddRange(publicKeysUsedToValidateSignature);
                    result.Keys.AddRange(publicKeysUsedForClientEncryption);
                    string json = JsonConvert.SerializeObject(result);
                    var data = Encoding.UTF8.GetBytes(json);
                    ctx.Response.ContentType = "application/json";
                    await ctx.Response.Body.WriteAsync(data, 0, data.Length);
                });
            });
            // 5. Use MVC.
            app.UseMvc();
        }

        private void ConfigureIdServer(IServiceCollection services)
        {
            services.AddHostIdentityServer(_options)
                .AddSimpleIdentityServerCore(_context.HttpClientFactory)
                .AddSimpleIdentityServerJwt()
                .AddIdServerLogging()
                .AddLogging()
                .AddSimpleIdentityServerInMemory();
        }

        private List<Dictionary<string, object>> ExtractPublicKeysForSignature(IEnumerable<JsonWebKey> jsonWebKeys)
        {
            var result = new List<Dictionary<string, object>>();
            var jsonWebKeysUsedForSignature = jsonWebKeys.Where(jwk => jwk.Use == Use.Sig && jwk.KeyOps.Contains(KeyOperations.Verify));
            foreach (var jsonWebKey in jsonWebKeysUsedForSignature)
            {
                var publicKeyInformation = _jsonWebKeyEnricher.GetPublicKeyInformation(jsonWebKey);
                var jsonWebKeyInformation = _jsonWebKeyEnricher.GetJsonWebKeyInformation(jsonWebKey);
                publicKeyInformation.AddRange(jsonWebKeyInformation);
                result.Add(publicKeyInformation);
            }

            return result;
        }

        private List<Dictionary<string, object>> ExtractPrivateKeysForSignature(IEnumerable<JsonWebKey> jsonWebKeys)
        {
            var result = new List<Dictionary<string, object>>();
            // Retrieve all the JWK used by the client to encrypt the JWS
            var jsonWebKeysUsedForEncryption =
                jsonWebKeys.Where(jwk => jwk.Use == Use.Enc && jwk.KeyOps.Contains(KeyOperations.Encrypt));
            foreach (var jsonWebKey in jsonWebKeysUsedForEncryption)
            {
                var publicKeyInformation = _jsonWebKeyEnricher.GetPublicKeyInformation(jsonWebKey);
                var jsonWebKeyInformation = _jsonWebKeyEnricher.GetJsonWebKeyInformation(jsonWebKey);
                publicKeyInformation.AddRange(jsonWebKeyInformation);
                result.Add(publicKeyInformation);
            }

            return result;
        }
    }
}
