using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using SimpleIdentityServer.Rfid.Client.Home.Views;
// using SimpleIdentityServer.Core.Jwt.Signature;

namespace SimpleIdentityServer.Rfid.Client.Home
{
    public class RfidHomeModule : IModule
    {
        private readonly IRegionViewRegistry _registry;
        private readonly IUnityContainer _container;

        public RfidHomeModule(IRegionViewRegistry registry, IUnityContainer container)
        {
            _registry = registry;
            _container = container;
        }

        public void Initialize()
        {
            // _container.RegisterType<IJwsParser, JwsParser>();
            _registry.RegisterViewWithRegion("MainRegion", typeof(HomeView));
        }
    }
}
