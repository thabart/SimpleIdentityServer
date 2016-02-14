using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.Facebook;
using Microsoft.AspNet.Authentication.MicrosoftAccount;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Host.MiddleWare;
using SimpleIdentityServer.Logging;
using Microsoft.AspNet.WebUtilities;
using Microsoft.AspNet.StaticFiles;
using Microsoft.AspNet.FileProviders;
using SimpleIdentityServer.Host.Controllers;
using System.Reflection;
using Microsoft.Extensions.Primitives;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections;

namespace SimpleIdentityServer.Host
{
    public class HostingOptions
    {
        /// <summary>
        /// Enable or disable the developer mode
        /// </summary>
        public bool IsDeveloperModeEnabled { get; set; }

        /// <summary>
        /// Gets or sets microsoft authentication enabled
        /// </summary>
        public bool IsMicrosoftAuthenticationEnabled { get; set; }

        /// <summary>
        /// Gets or sets the microsoft client id
        /// </summary>
        public string MicrosoftClientId { get; set; }

        /// <summary>
        /// Gets or sets the microsoft client secret
        /// </summary>
        public string MicrosoftClientSecret { get; set; }

        /// <summary>
        /// Gets or sets facebook authentication enabled
        /// </summary>
        public bool IsFacebookAuthenticationEnabled { get; set; }

        /// <summary>
        /// Gets or sets facebook client id
        /// </summary>
        public string FacebookClientId { get; set; }

        /// <summary>
        /// Gets or sets facebook client secret
        /// </summary>        
        public string FacebookClientSecret { get; set; }
    }

    public static class ApplicationBuilderExtensions 
    {
        
        #region Public static methods
        
        public static void UseSimpleIdentityServer(
            this IApplicationBuilder app,
            HostingOptions hostingOptions,
            SwaggerOptions swaggerOptions) 
        {
            if (hostingOptions == null)
            {
                throw new ArgumentNullException(nameof(hostingOptions));
            }

            if (swaggerOptions == null) {
                throw new ArgumentNullException(nameof(swaggerOptions));
            }

            app.UseIISPlatformHandler(opts => opts.AuthenticationDescriptions.Clear());

            var staticFileOptions = new StaticFileOptions();
            staticFileOptions.FileProvider = new CompositeFileProvider(
                    new EmbeddedFileProvider(
                        typeof(AuthenticateController).GetTypeInfo().Assembly,
                        "SimpleIdentityServer.Host.wwwroot"));
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
            if (hostingOptions.IsMicrosoftAuthenticationEnabled) 
            {                
                UseMicrosoftAuthentication(app, hostingOptions.MicrosoftClientId, hostingOptions.MicrosoftClientSecret);
            }

            // 4. Enable facebook authentication
            if (hostingOptions.IsFacebookAuthenticationEnabled)
            {
                UseFacebookAuthentication(app, hostingOptions.FacebookClientId, hostingOptions.FacebookClientSecret);
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

            if (swaggerOptions.IsSwaggerEnabled)
            {
                app.UseSwaggerGen();
                if (!string.IsNullOrWhiteSpace(swaggerOptions.SwaggerUrl))
                {
                    app.UseSwaggerUi(swaggerUrl : swaggerOptions.SwaggerUrl);
                }
                else
                {
                    app.UseSwaggerUi();
                }
            }        
        }
        
        #endregion
        
        #region Private static methods
        
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
        
        private static void UseFacebookAuthentication(            
            IApplicationBuilder app,
            string clientId,
            string clientSecret) 
        {
            var facebookOptions = new FacebookOptions
            {
                AuthenticationScheme = Constants.IdentityProviderNames.Facebook,
                DisplayName = Constants.IdentityProviderNames.Facebook,
                AppId = clientId,
                AppSecret = clientSecret,
                Scope = { "email" }
            };
            facebookOptions.Events =  new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    // 1. Fetch the user information from the user-information endpoint
                    var endPoint = QueryHelpers.AddQueryString(facebookOptions.UserInformationEndpoint, "access_token", context.AccessToken);
                    var response = await context.Backchannel.GetAsync(endPoint, context.HttpContext.RequestAborted);
                    response.EnsureSuccessStatusCode();
                    var payload = JObject.Parse(await response.Content.ReadAsStringAsync());

                    // 2. Retrieve the subject
                    var identifier = FacebookHelper.GetId(payload);
                    if (!string.IsNullOrEmpty(identifier))
                    {
                        context.Identity.AddClaim(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, 
                            identifier, 
                            ClaimValueTypes.String, 
                            facebookOptions.ClaimsIssuer));
                    }

                    // 3. Retrieve the email
                    var email = FacebookHelper.GetEmail(payload);
                    if (!string.IsNullOrEmpty(email))
                    {
                        context.Identity.AddClaim(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email,
                            email, 
                            ClaimValueTypes.String,
                            facebookOptions.ClaimsIssuer));
                    }

                    // 4. Retrieve the name
                    var name = FacebookHelper.GetName(payload);
                    if (!string.IsNullOrEmpty(name))
                    {
                        context.Identity.AddClaim(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name,
                            name, 
                            ClaimValueTypes.String,
                            facebookOptions.ClaimsIssuer));
                    }
                }
            };

            app.UseFacebookAuthentication(facebookOptions);
        }
        
        #endregion
    }
}