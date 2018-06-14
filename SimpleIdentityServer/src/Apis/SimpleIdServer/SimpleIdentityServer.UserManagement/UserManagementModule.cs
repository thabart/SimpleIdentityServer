using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Module;
using System.Collections.Generic;

namespace SimpleIdentityServer.UserManagement
{
    public class UserManagementModule : IModule
    {
        public static ModuleUIDescriptor ModuleUi = new ModuleUIDescriptor
        {
            Title = "UserManagement",
            RelativeUrl = "~/User",
            IsAuthenticated = true,
            Picture = "~/img/Unknown.png"
        };

        public void Configure(IApplicationBuilder applicationBuilder)
        {
        }

        public void Configure(IRouteBuilder routeBuilder)
        {
        }

        public void ConfigureAuthentication(AuthenticationBuilder authBuilder, IDictionary<string, string> options = null)
        {
        }

        public void ConfigureAuthorization(AuthorizationOptions authorizationOptions, IDictionary<string, string> options = null)
        {
        }

        public void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder = null, IHostingEnvironment env = null, IDictionary<string, string> options = null, IEnumerable<ModuleUIDescriptor> moduleUIDescriptors = null)
        {
        }

        public IEnumerable<string> GetOptionKeys()
        {
            return new string[0];
        }

        public ModuleUIDescriptor GetModuleUI()
        {
            return ModuleUi;
        }
    }
}
