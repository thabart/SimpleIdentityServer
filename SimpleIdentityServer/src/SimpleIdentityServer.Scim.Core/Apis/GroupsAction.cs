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
// limitations under the License.C:\Projects\SimpleIdentityServer\SimpleIdentityServer\src\SimpleIdentityServer.Scim.Core\DTOs\
#endregion

using Newtonsoft.Json.Linq;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IGroupsAction
    {
        JObject AddGroup(JObject jObj);
        JObject GetGroup(string id);
    }

    internal class GroupsAction : IGroupsAction
    {
        private readonly IAddRepresentationAction _addRepresentationAction;
        private readonly IGetRepresentationAction _getRepresentationAction;

        public GroupsAction(
            IAddRepresentationAction addRepresentationAction,
            IGetRepresentationAction getRepresentationAction)
        {
            _addRepresentationAction = addRepresentationAction;
            _getRepresentationAction = getRepresentationAction;
        }

        public JObject AddGroup(JObject jObj)
        {
            return _addRepresentationAction.Execute(jObj, Constants.SchemaUrns.Group, Constants.ResourceTypes.Group);
        }

        public JObject GetGroup(string id)
        {
            return _getRepresentationAction.Execute(id, Constants.SchemaUrns.Group, Constants.ResourceTypes.Group);
        }
    }
}
