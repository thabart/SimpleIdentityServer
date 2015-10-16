using System.ComponentModel.Composition;

using SimpleIdentityServer.Common;

using SwaggerLibrary.Helpers;

namespace SwaggerLibrary
{
    [Export(typeof(IModule))]
    public class ModuleInit : IModule
    {
        public void Initialize(IModuleRegistrar registrar)
        {
            registrar.RegisterType<ISwaggerDocumentationParser, SwaggerDocumentationParser>();
            registrar.RegisterType<IWebApiRequestor, WebApiRequestor>();
            registrar.RegisterType<IHttpClientHelper, HttpClientHelper>();
        }
    }
}
