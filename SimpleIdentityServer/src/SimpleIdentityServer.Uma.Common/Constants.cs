#region copyright
// Copyright 2016 Habart Thierry
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

namespace SimpleIdentityServer.Uma.Common
{
    public static class ResourceSetResponseNames
    {
        public const string Id = "_id";
        public const string Name = "name";
        public const string Uri = "uri";
        public const string Type = "type";
        public const string Scopes = "scopes";
        public const string IconUri = "icon_uri";
    }

    public static class AddResourceSetResponseNames
    {
        public const string Id = "_id";
        public const string UserAccessPolicyUri = "user_access_policy_uri";
    }

    public static class UpdateSetResponseNames
    {
        public const string Id = "_id";
    }

    public static class ClaimNames
    {
        public const string Type = "type";
        public const string Value = "value";
    }

    public static class PolicyRuleNames
    {
        public const string Id = "id";
        public const string ClientIdsAllowed = "clients";
        public const string IsResourceOwnerConsentNeeded = "consent_needed";
        public const string Scopes = "scopes";
        public const string Script = "script";
        public const string Claims = "claims";
    }

    public static class PolicyNames
    {
        public const string Id = "id";
        public const string ResourceSetIds = "resource_set_ids";
        public const string Rules = "rules";
    }

    public static class AddPolicyResponseNames
    {
        public const string PolicyId = "policy";
    }

    public static class PostAddResourceSetNames
    {
        public const string ResourceSets = "resources";
    }
}
