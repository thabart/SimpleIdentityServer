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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Client.Test
{
    class Program
    {
        #region Public methods

        public static void Main(string[] args)
        {
            var identityServerClientFactory = new IdentityServerClientFactory();
            var discoveryClient = identityServerClientFactory.CreateTokenClient()
                .UseClientSecretBasicAuth("51061382-2032-49c5-a059-055a9bf2e6c1", "7b936e81-f528-49a8-9468-04622ad54df0")
                .UseClientCredentials("uma_authorization", "uma_protection", "website_api", "uma")
                .ResolveAsync("https://localhost:54443/.well-known/openid-configuration");
            Console.ReadLine();
        }
        
        #endregion
    }
}
