using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using SimpleIdentityServer.Core.Jwt.Serializer;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Rfid.Client.Home.Views;

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
            _container.RegisterType<IJwsParser, JwsParser>();
            _container.RegisterType<ICreateJwsSignature, CreateJwsSignature>();
            _container.RegisterType<ICngKeySerializer, CngKeySerializer>();
            _registry.RegisterViewWithRegion("MainRegion", typeof(HomeView));
        }
    }
}
