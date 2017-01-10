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

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Test
{
    class Program
    {
        private const string _baseUrl = "https://rp.certification.openid.net:8080/simpleIdServer";
        
        public static void Main(string[] args)
        {
            // 1. Get token via client certificate authentication.
            // GetTokenViaClientCertificate().Wait();
            // 2. Execute tests for basic profile
            BasicProfile().Wait();
            // identityServerClientFactory.CreateAuthSelector()
            Console.ReadLine();
        }

        private static async Task BasicProfile()
        {
            var identityServerClientFactory = new IdentityServerClientFactory();
            var state = Guid.NewGuid().ToString();
            var nonce = Guid.NewGuid().ToString();
            // 1. rp-response_type-code : Make an authentication request using the "Authorization code Flow"
            var client = await identityServerClientFactory.CreateRegistrationClient()
                .ResolveAsync(new Core.Common.DTOs.Client
                {
                    RedirectUris = new List<string>
                    {
                        "https://localhost:5106/Authenticate/Callback"
                    },
                    ApplicationType = "web",
                    GrantTypes = new List<string>
                    {
                        "authorization_code"
                    },
                    ResponseTypes = new List<string>
                    {
                        "code"
                    },
                    JwksUri = "https://localhost:5106/jwks",
                }, _baseUrl + "/rp-response_type-code/.well-known/openid-configuration");
            // 2. Authorization request (with code)
            await identityServerClientFactory.CreateAuthorizationClient()
                .ResolveAsync(_baseUrl + "/rp-response_type-code/.well-known/openid-configuration",
                    new Core.Common.DTOs.AuthorizationRequest
                    {
                        ClientId = client.ClientId,
                        State = state,
                        RedirectUri = "https://localhost:5443/auth_cb",
                        ResponseType = "code",
                        Scope = "openid",
                        Nonce = nonce,
                        ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
                    });
        }

        public static async Task GetTokenViaClientCertificate()
        {
            var identityServerClientFactory = new IdentityServerClientFactory();
            var result = await identityServerClientFactory.CreateAuthSelector()
                .UseClientCertificate(new X509Certificate("LokitCa.cer"))
                .UsePassword("administrator", "password", "openid", "role")
                .ResolveAsync("https://localhost:5443/.well-known/openid-configuration");
        }
    }
}
