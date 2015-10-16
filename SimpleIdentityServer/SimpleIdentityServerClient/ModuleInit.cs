using System.ComponentModel.Composition;

using SimpleIdentityServer.Common;
using SimpleIdentityServerClient.Operations.Token;

namespace SimpleIdentityServerClient
{
    [Export(typeof(IModule))]
    public class ModuleInit : IModule
    {
        public void Initialize(IModuleRegistrar registrar)
        {
            registrar.RegisterType<IPostTokenOperation, PostTokenOperation>();
        }
    }
}
