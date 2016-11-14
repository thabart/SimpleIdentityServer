using Microsoft.Extensions.DependencyInjection;

namespace WebApiContrib.Core.Storage
{
    public class StorageOptionsBuilder
    {
        public StorageOptionsBuilder(IServiceCollection serviceCollection)
        {
            StorageOptions = new StorageOptions();
            ServiceCollection = serviceCollection;
        }

        public StorageOptions StorageOptions { get; private set; }
        public IServiceCollection ServiceCollection { get; private set; }
    }
}
