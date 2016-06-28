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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Serilog;
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using SimpleIdentityServer.Host.Controllers;
using SimpleIdentityServer.Host.MiddleWare;
using SimpleIdentityServer.Logging;
using System;
using System.Reflection;

namespace SimpleIdentityServer.Host
{
    public class HostingOptions
    {
        /// <summary>
        /// Enable or disable the developer mode
        /// </summary>
        public bool IsDeveloperModeEnabled { get; set; }

        /// <summary>
        /// Migrate the data
        /// </summary>
        public bool IsDataMigrated { get; set; }
    }

    public class AuthenticationOptions
    {
        public string ConfigurationUrl { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }

    public static class ApplicationBuilderExtensions 
    {        
        #region Public static methods
        
        public static void UseSimpleIdentityServer(this IApplicationBuilder app,
            Action<HostingOptions> hostingCallback,
            Action<SwaggerOptions> swaggerCallback,
            Action<AuthenticationOptions> authenticationOptionsCallback,
            ILoggerFactory loggerFactory) 
        {
            if (hostingCallback == null) 
            {
                throw new ArgumentNullException(nameof(hostingCallback));    
            }
            
            if (swaggerCallback == null) 
            {
                throw new ArgumentNullException(nameof(swaggerCallback));
            }

            if (authenticationOptionsCallback == null)
            {
                throw new ArgumentNullException(nameof(authenticationOptionsCallback));
            }
            
            var hostingOptions = new HostingOptions();
            var swaggerOptions = new SwaggerOptions();
            var authenticationOptions = new AuthenticationOptions();
            hostingCallback(hostingOptions);
            swaggerCallback(swaggerOptions);
            authenticationOptionsCallback(authenticationOptions);
            app.UseSimpleIdentityServer(hostingOptions,
                swaggerOptions, 
                authenticationOptions, 
                loggerFactory);
        }
        
        public static void UseSimpleIdentityServer(
            this IApplicationBuilder app,
            HostingOptions hostingOptions,
            SwaggerOptions swaggerOptions,
            AuthenticationOptions authenticationOptions,
            ILoggerFactory loggerFactory) 
        {
            if (hostingOptions == null)
            {
                throw new ArgumentNullException(nameof(hostingOptions));
            }

            if (swaggerOptions == null) {
                throw new ArgumentNullException(nameof(swaggerOptions));
            }


            if (authenticationOptions == null)
            {
                throw new ArgumentNullException(nameof(authenticationOptions));
            }

            var staticFileOptions = new StaticFileOptions();
            staticFileOptions.FileProvider = new EmbeddedFileProvider(
                        typeof(AuthenticateController).GetTypeInfo().Assembly,
                        "SimpleIdentityServer.Host.wwwroot");
            app.UseStaticFiles(staticFileOptions);           
            app.UseStatusCodePagesWithRedirects("~/Error/{0}");

            if (hostingOptions.IsDeveloperModeEnabled)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseSimpleIdentityServerExceptionHandler(new ExceptionHandlerMiddlewareOptions
                {
                    SimpleIdentityServerEventSource = app.ApplicationServices.GetService<ISimpleIdentityServerEventSource>()
                });
            }

            // 1. Configure the IUrlHelper extension
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            Extensions.UriHelperExtensions.Configure(httpContextAccessor); 

            // 2. Enable cookie authentication
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = new PathString("/Authenticate"),
                AuthenticationScheme = Constants.IdentityProviderNames.Cookies
            });

            // 3. Protect against IFRAME attack
            app.UseXFrame();

            // Check this implementation : https://github.com/aspnet/Security/blob/dev/samples/SocialSample/Startup.cs

            // 3. Enable authentication
            app.UseAuthentication(authenticationOptions);

            // 5. Migrate all the database
            if (hostingOptions.IsDataMigrated)
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var simpleIdentityServerContext = serviceScope.ServiceProvider.GetService<SimpleIdentityServerContext>();
                    simpleIdentityServerContext.Database.EnsureCreated();
                    simpleIdentityServerContext.EnsureSeedData();
                }
            }

            // 6. Add logging
            loggerFactory.AddSerilog();
            

            // 7. Configure ASP.NET MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute("Error401Route",
                    Constants.EndPoints.Get401,
                    new
                    {
                        controller = "Error",
                        action = "Get401"
                    });
                routes.MapRoute("Error404Route",
                    Constants.EndPoints.Get404,
                    new
                    {
                        controller = "Error",
                        action = "Get404"
                    });
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
        
        #endregion        
    }
}