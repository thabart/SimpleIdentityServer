using Microsoft.Extensions.DependencyInjection;

namespace SimpleIdentityServer.Manager.Core
{
    public static class AddSimpleIdentityServerManagerCoreExtensions
    {
        public static IServiceCollection AddSimpleIdentityServerManagerCore(this IServiceCollection serviceCollection)
        {
            return serviceCollection;
        }
    }
}
