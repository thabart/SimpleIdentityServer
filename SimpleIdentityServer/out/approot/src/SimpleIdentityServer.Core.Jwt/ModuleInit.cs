#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.ComponentModel.Composition;

using SimpleIdentityServer.Common;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Encrypt.Encryption;
using SimpleIdentityServer.Core.Jwt.Mapping;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.Jwt.Serializer;

namespace SimpleIdentityServer.Core.Jwt
{
    [Export(typeof(IModule))]
    public class ModuleInit : IModule
    {
        public void Initialize(IModuleRegister register)
        {
            register.RegisterType<IJweGenerator, JweGenerator>();
            register.RegisterType<IJweParser, JweParser>();
            register.RegisterType<IAesEncryptionHelper, AesEncryptionHelper>();
            register.RegisterType<IJweHelper, JweHelper>();

            register.RegisterType<IClaimsMapping, ClaimsMapping>();

            register.RegisterType<IJwsGenerator, JwsGenerator>();
            register.RegisterType<ICreateJwsSignature, CreateJwsSignature>();
            register.RegisterType<IJwsParser, JwsParser>();

            register.RegisterType<ICngKeySerializer, CngKeySerializer>();

            register.RegisterType<IJsonWebKeyConverter, JsonWebKeyConverter>();
        }
    }
}
