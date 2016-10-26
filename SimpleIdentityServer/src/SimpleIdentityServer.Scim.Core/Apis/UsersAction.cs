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

using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Core.Results;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IUsersAction
    {
        ApiActionResult AddUser(JObject jObj, string locationPattern);
        ApiActionResult UpdateUser(string id, JObject jObj, string locationPattern);
        ApiActionResult PatchUser(string id, JObject jObj, string locationPattern);
    }

    internal class UsersAction : IUsersAction
    {
        private readonly IAddRepresentationAction _addRepresentationAction;
        private readonly IUpdateRepresentationAction _updateRepresentationAction;
        private readonly IPatchRepresentationAction _patchRepresentationAction;

        public UsersAction(
            IAddRepresentationAction addRepresentationAction,
            IUpdateRepresentationAction updateRepresentationAction,
            IPatchRepresentationAction patchRepresentationAction)
        {
            _addRepresentationAction = addRepresentationAction;
            _updateRepresentationAction = updateRepresentationAction;
            _patchRepresentationAction = patchRepresentationAction;
        }

        public ApiActionResult AddUser(JObject jObj, string locationPattern)
        {
            return _addRepresentationAction.Execute(jObj, locationPattern, Constants.SchemaUrns.User, Constants.ResourceTypes.User);
        }

        public ApiActionResult UpdateUser(string id, JObject jObj, string locationPattern)
        {
            return _updateRepresentationAction.Execute(id, jObj, Constants.SchemaUrns.User, locationPattern, Constants.ResourceTypes.User);
        }

        public ApiActionResult PatchUser(string id, JObject jObj, string locationPattern)
        {
            return _patchRepresentationAction.Execute(id, jObj, Constants.SchemaUrns.User, locationPattern, Constants.ResourceTypes.User);
        }
    }
}
