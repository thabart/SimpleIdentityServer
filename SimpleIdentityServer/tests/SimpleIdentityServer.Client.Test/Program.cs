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
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Test
{
    class Program
    {
        private const string _baseUrl = "https://rp.certification.openid.net:8080/simpleIdServer";
        private const string RedirectUriCode = "https://localhost:5106/Authenticate/Callback";
        
        public static void Main(string[] args)
        {
            // 1. Execute tests for basic profile
            RpResponseTypeCode().Wait();
            RpScopeUserInfoClaims().Wait();
            // identityServerClientFactory.CreateAuthSelector()
            Console.ReadLine();
        }

        private static async Task RpResponseTypeCode()
        {
            var redirectUrl = RedirectUriCode + "RpResponseTypeCode";
            var identityServerClientFactory = new IdentityServerClientFactory();
            var state = Guid.NewGuid().ToString();
            var nonce = Guid.NewGuid().ToString();
            // rp-response_type-code : Make an authentication request using the "Authorization code Flow"
            var client = await identityServerClientFactory.CreateRegistrationClient()
                .ResolveAsync(new Core.Common.DTOs.Client
                {
                    RedirectUris = new List<string>
                    {
                        redirectUrl
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
            var result = await identityServerClientFactory.CreateAuthorizationClient()
                .ResolveAsync(_baseUrl + "/rp-response_type-code/.well-known/openid-configuration",
                    new Core.Common.DTOs.AuthorizationRequest
                    {
                        ClientId = client.ClientId,
                        State = state,
                        RedirectUri = redirectUrl,
                        ResponseType = "code",
                        Scope = "openid",
                        Nonce = nonce,
                        ResponseMode = Core.Common.DTOs.ResponseModes.Fragment
                    });
            Console.WriteLine(result.Location.AbsolutePath);
            // System.Diagnostics.Process.Start(result.Location.AbsolutePath);
        }

        private static async Task RpScopeUserInfoClaims()
        {
            var redirectUrl = RedirectUriCode + "RpScopeUserInfoClaims";
            var identityServerClientFactory = new IdentityServerClientFactory();
            // rp-scope-userinfo-claims : Request claims using scope values.
            var client = await identityServerClientFactory.CreateRegistrationClient()
                .ResolveAsync(new Core.Common.DTOs.Client
                {
                    RedirectUris = new List<string>
                    {
                        redirectUrl
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
            var state = Guid.NewGuid().ToString();
            var nonce = Guid.NewGuid().ToString();
            var result = await identityServerClientFactory.CreateAuthorizationClient()
                .ResolveAsync(_baseUrl + "/rp-response_type-code/.well-known/openid-configuration",
                    new Core.Common.DTOs.AuthorizationRequest
                    {
                        ClientId = client.ClientId,
                        State = state,
                        RedirectUri = redirectUrl,
                        ResponseType = "code",
                        Scope = "openid",
                        Nonce = nonce,
                        ResponseMode = Core.Common.DTOs.ResponseModes.Fragment
                    });
            Console.WriteLine(result.Location.AbsolutePath);
            // System.Diagnostics.Process.Start(result.Location.AbsolutePath);
        }
    }
}
