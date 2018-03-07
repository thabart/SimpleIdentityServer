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
using SimpleIdentityServer.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.ClientApplication
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.ReadLine();
            GetProtectedResource();
            Console.ReadLine();
        }

        private static async Task GetProtectedResource()
        {
            var httpClient = new HttpClient();
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:5000/Informations/Get/1")
            };
            var response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false); // 1. Try to retrieve protected resource without RPT token.
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var jObj = JObject.Parse(content);
            var ticketId = jObj["ticket_id"].ToString();
            var factory = new IdentityServerClientFactory();
            var grantedToken = await factory.CreateAuthSelector()   // 2. Retrieve the identity token.
                .UseClientSecretPostAuth("MedicalWebsite", "MedicalWebsite")
                .UsePassword("administrator", "password", "openid", "profile")
                .ResolveAsync("https://localhost:5443/.well-known/openid-configuration");
            var rptToken = await factory.CreateAuthSelector() // 3. Retrieve the RPT token.
                .UseClientSecretPostAuth("client", "client")
                .UseTicketId(ticketId, grantedToken.IdToken)
                .ResolveAsync("https://localhost:5445/.well-known/uma2-configuration");
            httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:5000/Informations/Get/1")
            };
            httpRequestMessage.Headers.Add("Authorization", "Bearer " + rptToken.AccessToken);
            response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            string s = "";
        }
    }
}
