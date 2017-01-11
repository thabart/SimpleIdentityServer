using Prism.Modularity;
using Prism.Regions;
using SimpleIdentityServer.Rfid.Client.Home.Views;

namespace SimpleIdentityServer.Rfid.Client.Home
{
    public class RfidHomeModule : IModule
    {
        private readonly IRegionViewRegistry _registry;

        public RfidHomeModule(IRegionViewRegistry registry)
        {
            _registry = registry;
        }

        public void Initialize()
        {
            _registry.RegisterViewWithRegion("MainRegion", typeof(HomeView));
        }
    }
}
