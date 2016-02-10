using System;
using System.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using SimpleIdentityServer.Api.Configuration;
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
using SimpleIdentityServer.Host.MiddleWare;
using SimpleIdentityServer.RateLimitation;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.MicrosoftAccount;
using Microsoft.AspNet.Authentication.OAuth;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace SimpleIdentityServer.Host 
{
    public static class SimpleIdentityServerHostExtensions 
    {
        #region Public static methods
        
        public static void AddSimpleIdentityServer(
            this IServiceCollection serviceCollection, 
            SimpleIdentityServerHostOptions options) 
        {
            if (options == null) {
                throw new ArgumentNullException("options");
            }
            
            if (options.DataSourceType == DataSourceTypes.InMemory) 
            {
                serviceCollection.AddSimpleIdentityServerFake();
                var clients = options.Clients.Select(c => c.ToFake()).ToList();
                var jsonWebKeys = options.JsonWebKeys.Select(j => j.ToFake()).ToList();
                var resourceOwners = options.ResourceOwners.Select(r => r.ToFake()).ToList();
                var scopes = options.Scopes.Select(s => s.ToFake()).ToList();
                var translations = options.Translations.Select(t => t.ToFake()).ToList();
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
                serviceCollection.AddTransient<SimpleIdentityServerContext>((a) => new SimpleIdentityServerContext(options.ConnectionString));
            }
            
            ConfigureSimpleIdentityServer(serviceCollection, options);
        }
        
        public static void UseSimpleIdentityServer(
            this IApplicationBuilder app,
            SimpleIdentityServerHostOptions options) 
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            app.UseIISPlatformHandler(opts => opts.AuthenticationDescriptions.Clear());

            app.UseStaticFiles();

            if (options.IsDeveloperModeEnabled)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseSimpleIdentityServerExceptionHandler(new ExceptionHandlerMiddlewareOptions
                {
                    SimpleIdentityServerEventSource = SimpleIdentityServerEventSource.Log
                });
            }
            
            // 1. Configure the IUrlHelper extension
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            SimpleIdentityServer.Host.Extensions.UriHelperExtensions.Configure(httpContextAccessor); 

            // 2. Enable cookie authentication
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = new PathString("/Authenticate")
            });

            // Check this implementation : https://github.com/aspnet/Security/blob/dev/samples/SocialSample/Startup.cs
            
            // 3. Enable live connect authentication
            if (options.IsMicrosoftAuthenticationEnabled) 
            {                
                UseMicrosoftAuthentication(app, options.MicrosoftClientId, options.MicrosoftClientSecret);
            }
            
            app.UseMvc(routes =>
            {
                routes.MapRoute("Error401Route",
                    Constants.EndPoints.Get401,
                    new
                    {
                        controller = "Error",
                        action = "Get401"
                    });

                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            if (options.IsSwaggerEnabled)
            {
                app.UseSwaggerGen();
                if (!string.IsNullOrWhiteSpace(options.SwaggerUrl))
                {
                    app.UseSwaggerUi(swaggerUrl : options.SwaggerUrl);
                }
                else
                {
                    app.UseSwaggerUi();
                }
            }        
        }
        
        #endregion
        
        
        #region Private static methods       
        
        /// <summary>
        /// Add all the dependencies needed to run Simple Identity Server
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        private static void ConfigureSimpleIdentityServer(
            IServiceCollection services,
            SimpleIdentityServerHostOptions options) 
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
            if (options.IsSwaggerEnabled)
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

            services.AddAuthentication(opts => opts.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
        }
        
        #region Different identity providers
        
        private static void UseMicrosoftAuthentication(
            IApplicationBuilder app,
            string clientId,
            string clientSecret) 
        {            
            var microsoftAccountOptions = new OAuthOptions
            {
                AuthenticationScheme = Constants.IdentityProviderNames.Microsoft,
                DisplayName = Constants.IdentityProviderNames.Microsoft,
                ClientId = clientId,
                ClientSecret = clientSecret,
                CallbackPath = new PathString("/signin-microsoft"),
                AuthorizationEndpoint = MicrosoftAccountDefaults.AuthorizationEndpoint,
                TokenEndpoint = MicrosoftAccountDefaults.TokenEndpoint,
                UserInformationEndpoint = MicrosoftAccountDefaults.UserInformationEndpoint,
                Scope = { "wl.basic" },
                SaveTokensAsClaims = true
            };
            
            microsoftAccountOptions.Events = new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    // 1. Fetch the user information from the user-information endpoint
                    var request = new HttpRequestMessage(HttpMethod.Get, microsoftAccountOptions.UserInformationEndpoint);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                    response.EnsureSuccessStatusCode();                    
                    var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
                    
                    // 2. Retrieve the subject
                    var identifier = MicrosoftAccountHelper.GetId(payload);
                    if (!string.IsNullOrWhiteSpace(identifier)) 
                    {
                        context.Identity.AddClaim(new Claim(
                            Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                            identifier, ClaimValueTypes.String, context.Options.ClaimsIssuer
                        ));
                    }
                    
                    // 3. Retrieve the name
                    var name = MicrosoftAccountHelper.GetName(payload);
                    if (!string.IsNullOrWhiteSpace(name)) 
                    {
                        context.Identity.AddClaim(new Claim(
                            Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name,
                            name, ClaimValueTypes.String, context.Options.ClaimsIssuer
                        ));
                    }
                    
                    // 3. Retrieve the email
                    var email = MicrosoftAccountHelper.GetEmail(payload);
                    if (!string.IsNullOrWhiteSpace(email)) {
                        context.Identity.AddClaim(new Claim(
                            Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email,
                            email, ClaimValueTypes.String, context.Options.ClaimsIssuer
                        ));
                    }
                }
            };
            
            app.UseOAuthAuthentication(microsoftAccountOptions);
        }
        
        #endregion
        
        #endregion
    }
}