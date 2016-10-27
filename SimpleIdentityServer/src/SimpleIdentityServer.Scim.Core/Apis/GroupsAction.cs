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

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Results;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IGroupsAction
    {
        ApiActionResult AddGroup(JObject jObj, string locationPattern);
        ApiActionResult GetGroup(string id, string locationPattern, IQueryCollection query);
        ApiActionResult RemoveGroup(string id);
        ApiActionResult UpdateGroup(string id, JObject jObj, string locationPattern);
        ApiActionResult PatchGroup(string id, JObject jObj, string locationPattern);
        ApiActionResult SearchGroups(JObject jObj);
        ApiActionResult SearchGroups(IQueryCollection query);
    }

    internal class GroupsAction : IGroupsAction
    {
        private readonly IAddRepresentationAction _addRepresentationAction;
        private readonly IGetRepresentationAction _getRepresentationAction;
        private readonly IDeleteRepresentationAction _deleteRepresentationAction;
        private readonly IUpdateRepresentationAction _updateRepresentationAction;
        private readonly IPatchRepresentationAction _patchRepresentationAction;
        private readonly ISearchParameterParser _searchParameterParser;
        private readonly IGetRepresentationsAction _getRepresentationsAction;

        public GroupsAction(
            IAddRepresentationAction addRepresentationAction,
            IGetRepresentationAction getRepresentationAction,
            IDeleteRepresentationAction deleteRepresentationAction,
            IUpdateRepresentationAction updateRepresentationAction,
            IPatchRepresentationAction patchRepresentationAction,
            ISearchParameterParser searchParameterParser,
            IGetRepresentationsAction getRepresentationsAction)
        {
            _addRepresentationAction = addRepresentationAction;
            _getRepresentationAction = getRepresentationAction;
            _deleteRepresentationAction = deleteRepresentationAction;
            _updateRepresentationAction = updateRepresentationAction;
            _patchRepresentationAction = patchRepresentationAction;
            _searchParameterParser = searchParameterParser;
            _getRepresentationsAction = getRepresentationsAction;
        }

        public ApiActionResult AddGroup(JObject jObj, string locationPattern)
        {
            return _addRepresentationAction.Execute(jObj, locationPattern, Constants.SchemaUrns.Group, Constants.ResourceTypes.Group);
        }

        public ApiActionResult GetGroup(string id, string locationPattern, IQueryCollection query)
        {
            var searchParameter = _searchParameterParser.ParseQuery(query);
            return _getRepresentationAction.Execute(id, locationPattern, Constants.SchemaUrns.Group, Constants.ResourceTypes.Group);
        }

        public ApiActionResult RemoveGroup(string id)
        {
            return _deleteRepresentationAction.Execute(id);
        }

        public ApiActionResult UpdateGroup(string id, JObject jObj, string locationPattern)
        {
            return _updateRepresentationAction.Execute(id, jObj, Constants.SchemaUrns.Group, locationPattern, Constants.ResourceTypes.Group);
        }

        public ApiActionResult PatchGroup(string id, JObject jObj, string locationPattern)
        {
            return _patchRepresentationAction.Execute(id, jObj, Constants.SchemaUrns.Group, locationPattern, Constants.ResourceTypes.Group);
        }

        public ApiActionResult SearchGroups(IQueryCollection query)
        {
            var searchParam = _searchParameterParser.ParseQuery(query);
            return _getRepresentationsAction.Execute(Constants.ResourceTypes.Group, searchParam);
        }

        public ApiActionResult SearchGroups(JObject jObj)
        {
            var searchParam = _searchParameterParser.ParseJson(jObj);
            return _getRepresentationsAction.Execute(Constants.ResourceTypes.Group, searchParam);
        }
    }
}
