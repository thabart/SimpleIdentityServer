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

namespace SimpleIdentityServer.Manager.Host
{
    public static class Constants
    {
        public static class EndPoints
        {
            public const string RootPath = "api";

            public const string Jws = RootPath + "/jws";
        }

        public static class GetJwsRequestNames
        {
            public const string Jws = "jws";

            public const string Url = "url";
        }

        public static class JwsInformationResponseNames
        {
            public const string Header = "header";

            public const string Payload = "payload";

            public const string JsonWebKey = "jsonwebkey";
        }

        public static class ErrorResponseNames
        {
            public const string Code = "code";

            public const string Message = "message";
        }

        public static class CreateJwsRequestNames
        {
            public const string Kid = "kid";

            public const string Alg = "alg";

            public const string Url = "url";

            public const string Payload = "payload";
        }
    }
}
