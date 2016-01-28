namespace SimpleIdentityServer.Common
{
    public interface IModuleRegister
    {
        void RegisterType<TFrom, TTo>() where TTo : TFrom;

        void RegisterInstance<TFrom>(TFrom obj);
    }
}
