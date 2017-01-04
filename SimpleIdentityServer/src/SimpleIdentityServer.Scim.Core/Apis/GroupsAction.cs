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
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IGroupsAction
    {
        Task<ApiActionResult> AddGroup(JObject jObj, string locationPattern);
        Task<ApiActionResult> GetGroup(string id, string locationPattern, IQueryCollection query);
        Task<ApiActionResult> RemoveGroup(string id);
        Task<ApiActionResult> UpdateGroup(string id, JObject jObj, string locationPattern);
        Task<ApiActionResult> PatchGroup(string id, JObject jObj, string locationPattern);
        Task<ApiActionResult> SearchGroups(JObject jObj, string locationPattern);
        Task<ApiActionResult> SearchGroups(IQueryCollection query, string locationPattern);
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

        public Task<ApiActionResult> AddGroup(JObject jObj, string locationPattern)
        {
            return _addRepresentationAction.Execute(jObj, locationPattern, Common.Constants.SchemaUrns.Group, Common.Constants.ResourceTypes.Group);
        }

        public Task<ApiActionResult> GetGroup(string id, string locationPattern, IQueryCollection query)
        {
            var searchParameter = _searchParameterParser.ParseQuery(query);
            return _getRepresentationAction.Execute(id, locationPattern, Common.Constants.SchemaUrns.Group);
        }

        public Task<ApiActionResult> RemoveGroup(string id)
        {
            return _deleteRepresentationAction.Execute(id);
        }

        public Task<ApiActionResult> UpdateGroup(string id, JObject jObj, string locationPattern)
        {
            return _updateRepresentationAction.Execute(id, jObj, Common.Constants.SchemaUrns.Group, locationPattern, Common.Constants.ResourceTypes.Group);
        }

        public Task<ApiActionResult> PatchGroup(string id, JObject jObj, string locationPattern)
        {
            return _patchRepresentationAction.Execute(id, jObj, Common.Constants.SchemaUrns.Group, locationPattern);
        }

        public Task<ApiActionResult> SearchGroups(IQueryCollection query, string locationPattern)
        {
            var searchParam = _searchParameterParser.ParseQuery(query);
            return _getRepresentationsAction.Execute(Common.Constants.ResourceTypes.Group, searchParam, locationPattern);
        }

        public Task<ApiActionResult> SearchGroups(JObject jObj, string locationPattern)
        {
            var searchParam = _searchParameterParser.ParseJson(jObj);
            return _getRepresentationsAction.Execute(Common.Constants.ResourceTypes.Group, searchParam, locationPattern);
        }
    }
}
