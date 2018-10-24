using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace SimpleIdServer.Storage.Tests
{
    public class InMemoryStorageFixture
    {
        private IStorage _storage;

        [Fact]
        public void When_Cache_Values_Then_Values_Can_Be_Retrieved()
        {
            // ARRANGE
            InitializeFakeObjects();
            var person = new Person
            {
                FirstName = "firstname"
            };

            // ACT
            _storage.Set("key", "val");
            _storage.Set("person", person);
            string firstVal = _storage.TryGetValue("key").ToString();
            var secondVal = _storage.TryGetValue<Person>("person");
            _storage.RemoveAll();
            var thirdResult = _storage.TryGetValue("key");


            // ASSERT
            Assert.Equal(firstVal, "val");
            Assert.NotNull(secondVal);
            Assert.Equal(secondVal.FirstName, "firstname");
            Assert.Null(thirdResult);
        }

        private class Person
        {
            public string FirstName { get; set; }
        }

        private void InitializeFakeObjects()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMemoryCache();
            serviceCollection.AddSingleton<IStorage, InMemoryStorage>();
            _storage = serviceCollection.BuildServiceProvider().GetService<IStorage>();
        }
    }
}
