namespace SimpleIdentityServer.Common
{
    public interface IModule
    {
        void Initialize(IModuleRegister register);
    }
}
