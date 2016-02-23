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

using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Oauth2Instrospection.Authentication.Tests.Fake
{
    public class FakeHttpMessageHandler : HttpClientHandler
    {
        private HttpResponseMessage _response;

        public static HttpMessageHandler GetHttpMessageHandler(string content, HttpStatusCode httpStatusCode)
        {
            var memStream = new MemoryStream();

            var sw = new StreamWriter(memStream);
            sw.Write(content);
            sw.Flush();
            memStream.Position = 0;

            var httpContent = new StreamContent(memStream);

            var response = new HttpResponseMessage()
            {
                StatusCode = httpStatusCode,
                Content = httpContent
            };

            var messageHandler = new FakeHttpMessageHandler(response);

            return messageHandler;
        }

        public FakeHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<HttpResponseMessage>();

            tcs.SetResult(_response);

            return tcs.Task;
        }
    }
}
