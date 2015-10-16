using System.ComponentModel.Composition;

using SimpleIdentityServer.Common;
using SimpleIdentityServer.Core.DataAccess;
using SimpleIdentityServer.Core.Helpers;

namespace SimpleIdentityServer.DataAccess.Fake
{
    [Export(typeof(IModule))]
    public class ModuleInit : IModule
    {
        public void Initialize(IModuleRegistrar registrar)
        {
            registrar.RegisterInstance<IDataSource>(new FakeDataSource(new SecurityHelper()));
        }
    }
}
