using System.ComponentModel.Composition;

using SimpleIdentityServer.Common;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Operations;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core
{
    [Export(typeof(IModule))]
    public class ModuleInit : IModule
    {
        public void Initialize(IModuleRegister registrar)
        {
            registrar.RegisterType<ISecurityHelper, SecurityHelper>();
            registrar.RegisterType<ITokenHelper, TokenHelper>();
            registrar.RegisterType<IClientValidator, ClientValidator>();
            registrar.RegisterType<IResourceOwnerValidator, ResourceOwnerValidator>();
            registrar.RegisterType<IScopeValidator, ScopeValidator>();

            registrar
                .RegisterType<IGetTokenByResourceOwnerCredentialsGrantType, GetTokenByResourceOwnerCredentialsGrantType>
                ();
            registrar
                .RegisterType<IGetAuthorizationCodeOperation, GetAuthorizationCodeOperation>
                ();
        }
    }
}
