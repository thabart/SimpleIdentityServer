using System.ComponentModel.Composition;

using SimpleIdentityServer.Common;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Operations;

namespace SimpleIdentityServer.Core
{
    [Export(typeof(IModule))]
    public class ModuleInit : IModule
    {
        public void Initialize(IModuleRegistrar registrar)
        {
            registrar.RegisterType<ISecurityHelper, SecurityHelper>();
            registrar.RegisterType<ITokenHelper, TokenHelper>();
            registrar.RegisterType<IValidatorHelper, ValidatorHelper>();

            registrar
                .RegisterType<IGetTokenByResourceOwnerCredentialsGrantType, GetTokenByResourceOwnerCredentialsGrantType>
                ();
        }
    }
}
