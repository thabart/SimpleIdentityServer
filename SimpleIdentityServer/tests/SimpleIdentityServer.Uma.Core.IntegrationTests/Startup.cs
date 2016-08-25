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

namespace SimpleIdentityServer.Uma.Core.IntegrationTests
{
    public class Startup
    {
        public void Start()
        {
            // IdTokenParser.ParseIdToken("eyJhbGciOiJSUzI1NiIsImtpZCI6IjEiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo1NDQzIiwiYXVkIjpbIkFub255bW91cyIsIlNpbXBsZUlkU2VydmVyQ2xpZW50IiwiaHR0cHM6Ly9sb2NhbGhvc3Q6NTQ0MyJdLCJleHAiOjE0NzA5NzM5ODguMCwiaWF0IjoxNDY3OTczOTg4LjAsIm5vbmNlIjoibm9uY2UiLCJhY3IiOiJvcGVuaWQucGFwZS5hdXRoX2xldmVsLm5zLnBhc3N3b3JkPTEiLCJhbXIiOlsicGFzc3dvcmQiXSwiYXpwIjoiU2ltcGxlSWRTZXJ2ZXJDbGllbnQiLCJzdWIiOiJBQUFBQUFBQUFBQUFBQUFBQUFBQUFINFJrdEZxeTFobW5xNUY4dE9QUnNBIiwicm9sZSI6IiIsIm5hbWUiOiJ0aGllcnJ5IGhhYmFydCIsInByZWZlcnJlZF91c2VybmFtZSI6ImhhYmFydGhpZXJyeUBob3RtYWlsLmZyIiwiYXRfaGFzaCI6IlhuQnBVZjJzTGRKXy0zcTNvTEF0UmcifQ.ZXlVOK2y73kD3_k0jTc_tBBTI_aEILBtasuLDmGcigelERK5YDZ3_wBLTWY0_ndlIGadFAfuNf4R657IBzYv7LsYs8ywZ6bgO-sl-sRrwnNbCC-uV2eCpDdWzgVBaZFg9cz92_oFEQsfCDPEgHMGipQ_41SyXIdsrJLFIur8Pec");

            var accessToken = "MzI3MjI4NDUtNmNjMi00M2Q3LWIxOGItMDc0ZGVlNDlhYWYy";
            var idToken = AuthProvider.GetIdentityToken();
            Console.WriteLine($"Id token : {idToken}");
            var rpt = SecurityProxy.GetRptToken(idToken, accessToken, accessToken);
            Console.WriteLine($"Rpt token : {rpt}");
        }
    }
}
