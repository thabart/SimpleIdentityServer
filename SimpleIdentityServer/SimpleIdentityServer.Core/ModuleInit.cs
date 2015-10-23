using System.ComponentModel.Composition;

using SimpleIdentityServer.Common;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Operations;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.Operations.Authorization;

namespace SimpleIdentityServer.Core
{
    [Export(typeof(IModule))]
    public class ModuleInit : IModule
    {
        public void Initialize(IModuleRegister register)
        {
            register.RegisterType<ISecurityHelper, SecurityHelper>();
            register.RegisterType<ITokenHelper, TokenHelper>();
            register.RegisterType<IClientValidator, ClientValidator>();
            register.RegisterType<IResourceOwnerValidator, ResourceOwnerValidator>();
            register.RegisterType<IScopeValidator, ScopeValidator>();
            register.RegisterType<IProtector, Protector.Protector>();
            register.RegisterType<ICompressor, Compressor>();


            register
                .RegisterType<IGetTokenByResourceOwnerCredentialsGrantType, GetTokenByResourceOwnerCredentialsGrantType>
                ();
            register
                .RegisterType<IGetAuthorizationOperation, GetAuthorizationOperation>
                ();
        }
    }
}
