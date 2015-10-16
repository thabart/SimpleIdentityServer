using Microsoft.Practices.Unity;

namespace SimpleIdentityServer.Common
{
    public class ModuleRegistrar : IModuleRegistrar
    {
        private readonly IUnityContainer _container;

        public ModuleRegistrar(IUnityContainer container)
        {
            _container = container;
        }

        public void RegisterType<TFrom, TTo>() where TTo : TFrom
        {
            _container.RegisterType<TFrom, TTo>();
        }
        
        public void RegisterInstance<TFrom>(TFrom obj)
        {
            _container.RegisterInstance<TFrom>(obj);
        }
    }
}
