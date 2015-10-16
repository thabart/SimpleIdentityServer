namespace SimpleIdentityServer.Common
{
    public interface IModuleRegistrar
    {
        void RegisterType<TFrom, TTo>() where TTo : TFrom;

        void RegisterInstance<TFrom>(TFrom obj);
    }
}
