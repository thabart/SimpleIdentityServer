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
        public void Start()
        {
            var factory = new SecurityProxyFactory();
            var proxy = factory.GetProxy(new SecurityOptions
            {
                ClientId = "SampleClient",
                ClientSecret = "SampleClient",
                UmaConfigurationUrl = "http://localhost:5001/.well-known/uma-configuration",
                OpenidConfigurationUrl = "http://localhost:5000/.well-known/openid-configuration",
                RootManageApiUrl = "http://localhost:8080/api"
            });
            try
            {
                var result = proxy.GetRpt("resources/first", new List<string>
                {
                    "read"
                }).Result;
                Console.WriteLine($"RPT token is {result}");
            }
            catch(AggregateException ex)
            {
                foreach(var inner in ex.InnerExceptions)
                {
                    Console.WriteLine(inner.Message);
                }
            }
        }
    }
}
