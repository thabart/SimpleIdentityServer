using System.ComponentModel.Composition;
using SimpleIdentityServer.Common;
using SimpleIdentityServer.Client.Operations.Token;

namespace SimpleIdentityServer.Client
{
    [Export(typeof(IModule))]
    public class ModuleInit : IModule
    {
        public void Initialize(IModuleRegister registrar)
        {
            registrar.RegisterType<IPostTokenOperation, PostTokenOperation>();
        }
    }
}
