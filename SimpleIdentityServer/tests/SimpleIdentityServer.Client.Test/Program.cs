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
            // const string json = "{\"nbf\":1473789424,\"exp\":1473793024,\"iss\":\"https://localhost:5443\",\"aud\":\"https://localhost:5443/resources\",\"client_id\":\"ManagerWebSiteApi\",\"scope\":\"manage_configuration\",\"active\":true}";
            const string json = "{\"nbf\":1473789424,\"exp\":1473793024,\"iss\":\"https://localhost:5443\",\"aud\":\"https://localhost:5443/resources\",\"client_id\":\"ManagerWebSiteApi\",\"scope\":[\"manage_configuration\", \"manage_configuration\"],\"active\":true}";
            var jObj = JObject.Parse(json);
            JToken token;
            bool active;
            var scopes = new List<string>();
            if (jObj.TryGetValue("active", out token))
            {
                active = bool.Parse(token.ToString());
            }

            if (jObj.TryGetValue("scope", out token))
            {
                if (token.Children().Count() > 0)
                {
                    foreach(var scope in token.Children())
                    {
                        scopes.Add(scope.ToString());
                    }
                }
                else
                {
                    scopes.Add(token.ToString());
                }
            }
            /*
            var identityServerClientFactory = new IdentityServerClientFactory();
            var discoveryClient = identityServerClientFactory.CreateDiscoveryClient();
            var discoveryInformation = discoveryClient.GetDiscoveryInformation("http://localhost:5000/.well-known/openid-configuration");
            Console.WriteLine(discoveryInformation.AuthorizationEndPoint);
            */
            Console.ReadLine();
        }
        
        #endregion
    }
}
