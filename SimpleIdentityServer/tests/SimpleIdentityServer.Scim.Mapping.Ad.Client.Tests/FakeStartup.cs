using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Scim.Mapping.Ad.Client.Tests.Extensions;
using SimpleIdentityServer.Scim.Mapping.Ad.Controllers;
using SimpleIdentityServer.Scim.Mapping.Ad.InMemory;
using System.Reflection;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Tests
{
    public class FakeStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("scim_manage", policy => policy.RequireAssertion((ctx) => {
                    return true;
                }));
            });
            services.AddScimMapping();
            services.AddScimMappingInMemoryEF();
            var mvc = services.AddMvc();
            var parts = mvc.PartManager.ApplicationParts;
            parts.Clear();
            parts.Add(new AssemblyPart(typeof(AdConfigurationController).GetTypeInfo().Assembly));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            InitializeDatabase(app);
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MappingDbContext>();
                context.EnsureSeedData();
            }
        }
    }
}
