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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SimpleIdentityServer.EventStore.Host.Extensions
{
    public static class HttpRequestsExtensions
    {
        public static string GetAbsoluteUriWithVirtualPath(this HttpRequest requestMessage)
        {
            var host = requestMessage.Host.Value;
            var http = "http://";
            if (requestMessage.IsHttps)
            {
                http = "https://";
            }

            var relativePath = requestMessage.PathBase.Value;
            return http + host + relativePath;
        }
        
        public static async Task<string> ReadAsStringAsync(this HttpRequest request) {
            request.Body.Position = 0;
            using (var reader = new StreamReader(request.Body)) 
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}