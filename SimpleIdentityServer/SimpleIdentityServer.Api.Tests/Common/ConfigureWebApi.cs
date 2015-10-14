using Microsoft.Practices.Unity;

using RateLimitation.Configuration;

using SimpleIdentityServer.Core.DataAccess;

namespace SimpleIdentityServer.Api.Tests.Common
{
    public class ConfigureWebApi
    {
        public ConfigureWebApi()
        {
            DataSource = new FakeDataSource();
            GetRateLimitationElementOperation = new GetRateLimitationElementOperation();
            ConfigureUnityContainer();
        }

        public ConfigureWebApi(IGetRateLimitationElementOperation getRateLimitationElementOperation)
        {
            GetRateLimitationElementOperation = getRateLimitationElementOperation;
            DataSource = new FakeDataSource();
            ConfigureUnityContainer();
        }

        public IDataSource DataSource { get; private set; }

        public IGetRateLimitationElementOperation GetRateLimitationElementOperation { get; private set; }

        private void ConfigureUnityContainer()
        {
            UnityConfig.SetRegisterDependenciesCallback(RegisterFakeDependencies);
        }

        private void RegisterFakeDependencies(UnityContainer container)
        {
            container.RegisterInstance<IDataSource>(DataSource);
            container.RegisterInstance<IGetRateLimitationElementOperation>(GetRateLimitationElementOperation);
        }
    }
}
