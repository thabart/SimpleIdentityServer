using System.ComponentModel.Composition;

using SimpleIdentityServer.Common;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Encrypt.Encryption;
using SimpleIdentityServer.Core.Jwt.Mapping;
using SimpleIdentityServer.Core.Jwt.Signature;

namespace SimpleIdentityServer.Core.Jwt
{
    [Export(typeof(IModule))]
    public class ModuleInit : IModule
    {
        public void Initialize(IModuleRegister register)
        {
            register.RegisterType<IJweGenerator, JweGenerator>();
            register.RegisterType<IAesEncryptionHelper, AesEncryptionHelper>();

            register.RegisterType<IClaimsMapping, ClaimsMapping>();

            register.RegisterType<IJwsGenerator, IJwsGenerator>();
            register.RegisterType<ICreateJwsSignature, CreateJwsSignature>();
            register.RegisterType<IJwsParser, JwsParser>();
        }
    }
}
