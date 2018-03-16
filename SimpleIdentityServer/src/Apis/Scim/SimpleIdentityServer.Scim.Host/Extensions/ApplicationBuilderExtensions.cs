using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleIdentityServer.Scim.Db.EF;
using SimpleIdentityServer.Scim.Host.Configurations;
using System;

namespace SimpleIdentityServer.Scim.Host.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseScimHost(this IApplicationBuilder app, ILoggerFactory loggerFactory, ScimConfiguration configuration)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (loggerFactory != null)
            {
                loggerFactory.AddConsole();
            }

            // 1. Display status code page.
            app.UseStatusCodePages();
			/*
            // 2. Enable OAUTH authentication.
            var introspectionOptions = new Oauth2IntrospectionOptions
            {
                InstrospectionEndPoint = configuration.AuthorizationServer.IntrospectionEndpoints,
                ClientId = configuration.AuthorizationServer.ClientId,
                ClientSecret = configuration.AuthorizationServer.ClientSecret
            };
            app.UseAuthenticationWithIntrospection(introspectionOptions);
			*/
            // 3. Insert seed data
            if (configuration.DataSource.IsDataMigrated)
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var scimDbContext = serviceScope.ServiceProvider.GetService<ScimDbContext>();
                    try
                    {
                        scimDbContext.Database.EnsureCreated();
                    }
                    catch (Exception) { }
                }
            }

            // 4. Enable CORS
            app.UseCors("AllowAll");
            // 5. Launch ASP.NET MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            return app;
        }
    }
}
