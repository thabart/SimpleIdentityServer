using Microsoft.Practices.Unity;

using SimpleIdentityServer.Core.DataAccess;

namespace SimpleIdentityServer.Api.Tests.Common
{
    public class ConfigureWebApi
    {
        public ConfigureWebApi()
        {
            DataSource = new FakeDataSource();
            ConfigureUnityContainer();
        }

        public IDataSource DataSource { get; private set; }

        private void ConfigureUnityContainer()
        {
            UnityConfig.SetRegisterDependenciesCallback(RegisterFakeDependencies);
        }

        private void RegisterFakeDependencies(UnityContainer container)
        {
            container.RegisterInstance<IDataSource>(DataSource);
        }
    }
}
