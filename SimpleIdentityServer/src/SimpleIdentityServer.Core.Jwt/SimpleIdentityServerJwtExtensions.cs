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

using Microsoft.Extensions.DependencyInjection;

using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Encrypt.Encryption;
using SimpleIdentityServer.Core.Jwt.Mapping;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.Jwt.Serializer;

namespace SimpleIdentityServer.Core.Jwt
{
    public static class SimpleIdentityServerJwtExtensions
    {
        public static IServiceCollection AddSimpleIdentityServerJwt(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IJweGenerator, JweGenerator>();
            serviceCollection.AddTransient<IJweParser, JweParser>();
            serviceCollection.AddTransient<IAesEncryptionHelper, AesEncryptionHelper>();
            serviceCollection.AddTransient<IJweHelper, JweHelper>();
            serviceCollection.AddTransient<IClaimsMapping, ClaimsMapping>();
            serviceCollection.AddTransient<IJwsGenerator, JwsGenerator>();
            serviceCollection.AddTransient<ICreateJwsSignature, CreateJwsSignature>();
            serviceCollection.AddTransient<IJwsParser, JwsParser>();
#if NET46
            serviceCollection.AddTransient<ICngKeySerializer, CngKeySerializer>();
#endif
            serviceCollection.AddTransient<IJsonWebKeyConverter, JsonWebKeyConverter>();
            return serviceCollection;
        }
    }
}
