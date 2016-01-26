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
        }
    }
}
