using Microsoft.Extensions.DependencyInjection;

namespace WebApiContrib.Core.Storage
{
    public class StorageOptionsBuilder
    {
        public StorageOptions StorageOptions { get; private set; }

        public IServiceCollection ServiceCollection { get; private set; }

        public StorageOptionsBuilder(IServiceCollection serviceCollection)
        {
            StorageOptions = new StorageOptions();
            ServiceCollection = serviceCollection;
        }

        public void UseInMemoryStorage()
        {
            StorageOptions.Storage = new InMemoryStorage();
        }
    }
}
