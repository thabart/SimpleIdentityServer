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

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http;

namespace CustomerPortal
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; set; }
        
        public Startup(IHostingEnvironment env)
        {
            // Load all the configuration information from the "json" file & the environment variables.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // 1. Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // 2. Add the dependencies to run ASP.NET MVC
            services.AddMvc();
            // 3. Add authentication
            services.AddAuthentication(opts => opts.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
            services.AddAuthorization(opts =>
            {
                opts.AddPolicy("Connected", policy => policy.RequireAssertion((ctx) => {
                    return ctx.User.Identity != null && ctx.User.Identity.IsAuthenticated;
                }));
            });
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            // 1. Display status code page
            app.UseStatusCodePages();
            // 2. Enable CORS
            app.UseCors("AllowAll");
            // 3. Use static files
            app.UseStaticFiles();
            // 4. Use cookie authentication.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = new PathString("/Authenticate")
            });
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "External-CustomerPortal"
            });
            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                AuthenticationScheme = Constants.RfidProvider,
                SignInScheme = Constants.ExternalCookieName,
                DisplayName = "RFID",
                ClientId = "CustomerPortal",
                ClientSecret = "CustomerPortal",
                MetadataAddress = "http://localhost:5100/.well-known/openid-configuration",
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = true,
                    ValidAudience = "CustomerPortal"
                },
                ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>("http://localhost:5100/.well-known/openid-configuration",
                        new OpenIdConnectConfigurationRetriever(),
                        new HttpDocumentRetriever(new HttpClient()) { RequireHttps = false })
            });
            app.UseMicrosoftAccountAuthentication(new MicrosoftAccountOptions
            {
                DisplayName = "MicrosoftAccount",
                AuthenticationScheme = Constants.MicrosoftProvider,
                ClientId = "f152ac01-1d88-4f2a-9d9a-49d0023e836f",
                ClientSecret = "Mns9R7fgARynJQR2dtAGznq",
                SaveTokens = true,
                SignInScheme = Constants.ExternalCookieName,
            });
            // 5. Launch ASP.NET MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
