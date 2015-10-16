namespace SimpleIdentityServer.Common
{
    public interface IModule
    {
        void Initialize(IModuleRegistrar registrar);
    }
}
