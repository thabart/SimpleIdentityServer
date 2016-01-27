using System.ComponentModel.Composition;

using SimpleIdentityServer.Common;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Repositories;

namespace SimpleIdentityServer.DataAccess.SqlServer
{
    [Export(typeof(IModule))]
    public sealed class ModuleInit
    {
        public void Initialize(IModuleRegister register)
        {
            register.RegisterType<ITranslationRepository, TranslationRepository>();
            register.RegisterType<IResourceOwnerRepository, ResourceOwnerRepository>();
            register.RegisterType<IScopeRepository, ScopeRepository>();
            register.RegisterType<IAuthorizationCodeRepository, AuthorizationCodeRepository>();
            register.RegisterType<IClientRepository, ClientRepository>();
            register.RegisterType<IConsentRepository, ConsentRepository>();
            register.RegisterType<IGrantedTokenRepository, GrantedTokenRepository>();
            register.RegisterType<IJsonWebKeyRepository, JsonWebKeyRepository>();
        }
    }
}
