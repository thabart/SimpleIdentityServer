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

using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Core
{
    public static class Constants
    {
        #region DTO names

        #endregion

        public static class RoutePaths
        {
            public const string GroupsController = "Groups";
            public const string UsersController = "Users";
            public const string SchemasController = "Schemas";
            public const string ServiceProviderConfigController = "ServiceProviderConfig";
            public const string BulkController = "Bulk";
        }

        public static Dictionary<string, string> MappingRoutePathsToResourceTypes = new Dictionary<string, string>
        {
            {
                RoutePaths.UsersController,
                Common.Constants.ResourceTypes.User
            },
            {
                RoutePaths.GroupsController,
                Common.Constants.ResourceTypes.Group
            }
        };
    }
}
