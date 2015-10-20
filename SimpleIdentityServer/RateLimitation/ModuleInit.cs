using System.ComponentModel.Composition;
using SimpleIdentityServer.Common;
using SimpleIdentityServer.RateLimitation.Configuration;

namespace SimpleIdentityServer.RateLimitation
{
    [Export(typeof(IModule))]
    public class ModuleInit : IModule
    {
        public void Initialize(IModuleRegister registrar)
        {
            registrar.RegisterType<IGetRateLimitationElementOperation, GetRateLimitationElementOperation>();
        }
    }
}
