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

using SimpleIdentityServer.Core.Common;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace SimpleIdentityServer.Client.Builders
{
    public class RequestBuilder
    {
        public RequestBuilder()
        {
            Content = new Dictionary<string, string>();
            Certificate = null;
        }

        public string AuthorizationHeaderValue { get; set; }
        public X509Certificate2 Certificate { get; set; }
        public Dictionary<string, string> Content { get; private set; }

        public RequestBuilder SetClientCredentials(string clientId, string clientSecret)
        {
            Content.Add(ClientAuthNames.ClientId, clientId);
            Content.Add(ClientAuthNames.ClientSecret, clientSecret);
            return this;
        }

        public RequestBuilder SetClientAssertion(string clientId, string clientAssertion, string clientAssertionType)
        {
            Content.Add(ClientAuthNames.ClientId, clientId);
            Content.Add(ClientAuthNames.ClientAssertion, clientAssertion);
            Content.Add(ClientAuthNames.ClientAssertionType, clientAssertionType);
            return this;
        }

        public RequestBuilder SetCertificate(string clientId, X509Certificate2 certificate)
        {
            Content.Add(ClientAuthNames.ClientId, clientId);
            Certificate = certificate;
            return this;
        }
    }
}
