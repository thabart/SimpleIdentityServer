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
            public const string RootPath = "/api";
            public const string Jws = RootPath + "/jws";
            public const string Jwe = RootPath + "/jwe";
            public const string Clients = RootPath + "/clients";
            public const string Scopes = RootPath + "/scopes";
            public const string ResourceOwners = RootPath + "/resource_owners";
            public const string Manage = RootPath + "/manage";
            public const string Configuration = ".well-known/openidmanager-configuration";
        }

        public static class ConfigurationNames
        {
            public const string JwsEndpoint = "jws_endpoint";
            public const string JweEndpoint = "jwe_endpoint";
            public const string ClientsEndpoint = "clients_endpoint";
            public const string ScopesEndpoint = "scopes_endpoint";
            public const string ResourceOwnersEndpoint = "resourceowners_endpoint";
            public const string ManageEndpoint = "manage_endpoint";
        }
    }
}
