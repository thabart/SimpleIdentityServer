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

namespace SimpleIdentityServer.Global.Startup
{
    public static class Constants
    {
        public const string AuthorizationUrlName = "AuthorizationUrl";

        public const string TokenUrlName = "TokenUrl";

        public const string UserInformationUrlName = "UserInfoUrl";

        public const string ConnectionStringName = "Data:DefaultConnection:ConnectionString";

        public const string IsSqlServerName = "isSqlServer";

        public const string IsSqlLiteName = "isSqlLite";

        public const string MicrosoftClientIdName = "Microsoft:ClientId";

        public const string MicrosoftClientSecret = "Microsoft:ClientSecret";

        public const string FacebookClientIdName = "Facebook:ClientId";

        public const string FacebookClientSecretName = "Facebook:ClientSecret";
    }
}
