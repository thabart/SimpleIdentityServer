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
    }

    internal class GroupsAction : IGroupsAction
    {
        private readonly IAddRepresentationAction _addRepresentationAction;

        public GroupsAction(IAddRepresentationAction addRepresentationAction)
        {
            _addRepresentationAction = addRepresentationAction;
        }

        public JObject AddGroup(JObject jObj)
        {
            return _addRepresentationAction.Execute(jObj, Constants.SchemaUrns.Group, "Group");
        }
    }
}
