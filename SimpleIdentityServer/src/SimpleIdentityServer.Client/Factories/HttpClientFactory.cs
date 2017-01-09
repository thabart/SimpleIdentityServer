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

#if NET
using System.Net;
#endif
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace SimpleIdentityServer.Client.Factories
{
    public interface IHttpClientFactory
    {
        HttpClient GetHttpClient();
        HttpClient GetHttpClient(X509Certificate certificate);
    }

    internal sealed class HttpClientFactory : IHttpClientFactory
    {
        public HttpClient GetHttpClient()
        {
            var httpHandler = new HttpClientHandler();
#if NET
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
#else
            httpHandler.ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true;
#endif
            return new HttpClient(httpHandler);
        }

        public HttpClient GetHttpClient(X509Certificate certificate)
        {
#if NET
            var handler = new WebRequestHandler();
            handler.ClientCertificates.Add(certificate);
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            return new HttpClient(handler);
#else
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(certificate);
            handler.ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true;
            return new HttpClient(handler);
#endif
        }
    }
}
