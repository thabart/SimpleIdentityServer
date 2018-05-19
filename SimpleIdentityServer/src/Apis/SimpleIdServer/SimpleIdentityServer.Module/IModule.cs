using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleIdentityServer.Module
{
    public interface IModule
    {
        void ConfigureServices(IServiceCollection services, IMvcBuilder mvcBuilder = null, IHostingEnvironment env = null);
        void Configure(IApplicationBuilder applicationBuilder);
    }
}
