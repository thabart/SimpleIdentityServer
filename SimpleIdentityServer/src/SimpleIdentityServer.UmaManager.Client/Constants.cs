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

namespace SimpleIdentityServer.UmaManager.Client
{
    public static class Constants
    {
        public static class ResourceResponseNames
        {
            public const string Url = "url";

            public const string Hash = "hash";

            public const string AuthorizationPolicy = "authorization_policy";

            public const string Policy = "policy";

            public const string ResourceSetId = "resource_set_id";
        }

        public static class PolicyResponseNames
        {
            public const string Claims = "claims";

            public const string Scopes = "scopes";

            public const string AllowedClients = "allowed_clients";
        }

        public static class ClaimResponseNames
        {
            public const string Type = "type";

            public const string Value = "value";
        }

        public static class AddControllerActionRequestNames
        {
            public const string Application = "application";

            public const string Version = "version";

            public const string Controller = "controller";

            public const string Action = "action";
        }
    }
}
