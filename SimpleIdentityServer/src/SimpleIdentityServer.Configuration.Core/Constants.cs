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

namespace SimpleIdentityServer.Configuration.Core
{
    public static class Constants
    {
        public static class AuthProviderResponseNames
        {
            public const string Name = "name";

            public const string IsEnabled = "is_enabled";

            public const string Options = "options";
        }

        public static class OptionResponseNames
        {
            public const string Id = "_id";

            public const string Key = "key";

            public const string Value = "value";
        }

        public static class SettingNames
        {
            public const string ExpirationTimeName = "TokenExpirationTime";
            public const string AuthorizationCodeExpirationTimeName = "AuthorizationCodeExpirationTime";
        }
    }
}
