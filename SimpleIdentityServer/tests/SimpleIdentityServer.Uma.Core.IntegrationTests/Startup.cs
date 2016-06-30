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

using SimpleIdentityServer.Proxy;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Core.IntegrationTests
{
    public class Startup
    {
        private const string UmaConfigurationUrl = "http://localhost:5001/.well-known/uma-configuration";

        private const string OpenIdConfigurationUrl = "http://localhost:5000/.well-known/openid-configuration";

        private const string RootManageApiUrl = "http://localhost:8080/api";

        public void Start()
        {
            var idToken = GetIdentityToken();
            Console.WriteLine($"Id token : {idToken}");
            var rpt = GetRptToken(idToken);
            Console.WriteLine($"Rpt token : {rpt}");
        }

        public static string GetIdentityToken()
        {
            var authProvider = new AuthenticationProviderFactory();
            var options = new AuthOptions
            {
                OpenIdConfigurationUrl = OpenIdConfigurationUrl,
                ClientId = "Anonymous",
                ClientSecret = "Anonymous"
            };


            try
            {
                return authProvider.GetAuthProvider(options)
                    .GetIdentityToken("administrator", "password", "openid", "role", "profile")
                    .Result;
            }
            catch (AggregateException ex)
            {
                return null;
            }
        }

        public static string GetRptToken(string identityToken)
        {
            var factory = new SecurityProxyFactory();
            var proxy = factory.GetProxy(new SecurityOptions
            {
                ClientId = "SampleClient",
                ClientSecret = "SampleClient",
                UmaConfigurationUrl = UmaConfigurationUrl,
                OpenidConfigurationUrl = OpenIdConfigurationUrl,
                RootManageApiUrl = RootManageApiUrl
            });
            try
            {
                var result = proxy.GetRpt("resources/Apis/operation", identityToken, new List<string>
                {
                    "execute"
                }).Result;
                return result;
            }
            catch (AggregateException)
            {
                return null;
            }
        }
    }
}
