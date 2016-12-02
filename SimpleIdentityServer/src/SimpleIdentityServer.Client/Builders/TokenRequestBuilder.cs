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
using SimpleIdentityServer.Core.Common;
using System.Collections.Generic;

namespace SimpleIdentityServer.Client.Builders
{
    public class RequestBuilder
    {
        public RequestBuilder()
        {
            Content = new Dictionary<string, string>();
        }

        public string AuthorizationHeaderValue { get; set; }
        public Dictionary<string, string> Content { get; private set; }

        public RequestBuilder SetClientCredentials(string clientId, string clientSecret)
        {
            Content.Add(ClientAuthNames.ClientId, clientId);
            Content.Add(ClientAuthNames.ClientSecret, clientId);
            return this;
        }

        public RequestBuilder SetClientAssertion(string clientId, string clientAssertion, string clientAssertionType)
        {
            Content.Add(ClientAuthNames.ClientId, clientId);
            Content.Add(ClientAuthNames.ClientAssertion, clientAssertion);
            Content.Add(ClientAuthNames.ClientAssertionType, clientAssertionType);
            return this;
        }
    }
}
