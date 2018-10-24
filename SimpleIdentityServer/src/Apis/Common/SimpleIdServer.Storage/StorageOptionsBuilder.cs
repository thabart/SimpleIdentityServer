using Microsoft.Extensions.DependencyInjection;

namespace SimpleIdServer.Storage
{
    public class StorageOptionsBuilder
    {
        public StorageOptionsBuilder(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;
        }

        public IServiceCollection ServiceCollection { get; private set; }
    }
}
