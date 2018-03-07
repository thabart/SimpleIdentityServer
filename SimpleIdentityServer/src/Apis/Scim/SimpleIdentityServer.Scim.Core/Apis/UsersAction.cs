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

using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Core.Results;
using SimpleIdentityServer.Scim.Core.Parsers;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IUsersAction
    {
        Task<ApiActionResult> AddUser(JObject jObj, string locationPattern);
        Task<ApiActionResult> UpdateUser(string id, JObject jObj, string locationPattern);
        Task<ApiActionResult> PatchUser(string id, JObject jObj, string locationPattern);
        Task<ApiActionResult> RemoveUser(string id);
        Task<ApiActionResult> GetUser(string id, string locationPattern);
        Task<ApiActionResult> SearchUsers(JObject jObj, string locationPattern);
        Task<ApiActionResult> SearchUsers(IQueryCollection query, string locationPattern);
    }

    internal class UsersAction : IUsersAction
    {
        private readonly IAddRepresentationAction _addRepresentationAction;
        private readonly IUpdateRepresentationAction _updateRepresentationAction;
        private readonly IPatchRepresentationAction _patchRepresentationAction;
        private readonly IDeleteRepresentationAction _deleteRepresentationAction;
        private readonly IGetRepresentationAction _getRepresentationAction;
        private readonly IGetRepresentationsAction _getRepresentationsAction;
        private readonly ISearchParameterParser _searchParameterParser;

        public UsersAction(
            IAddRepresentationAction addRepresentationAction,
            IUpdateRepresentationAction updateRepresentationAction,
            IPatchRepresentationAction patchRepresentationAction,
            IDeleteRepresentationAction deleteRepresentationAction,
            IGetRepresentationAction getRepresentationAction,
            IGetRepresentationsAction getRepresentationsAction,
            ISearchParameterParser searchParameterParser)
        {
            _addRepresentationAction = addRepresentationAction;
            _updateRepresentationAction = updateRepresentationAction;
            _patchRepresentationAction = patchRepresentationAction;
            _deleteRepresentationAction = deleteRepresentationAction;
            _getRepresentationAction = getRepresentationAction;
            _getRepresentationsAction = getRepresentationsAction;
            _searchParameterParser = searchParameterParser;
        }

        public Task<ApiActionResult> AddUser(JObject jObj, string locationPattern)
        {
            return _addRepresentationAction.Execute(jObj, locationPattern, Common.Constants.SchemaUrns.User, Common.Constants.ResourceTypes.User);
        }

        public Task<ApiActionResult> UpdateUser(string id, JObject jObj, string locationPattern)
        {
            return _updateRepresentationAction.Execute(id, jObj, Common.Constants.SchemaUrns.User, locationPattern, Common.Constants.ResourceTypes.User);
        }

        public Task<ApiActionResult> PatchUser(string id, JObject jObj, string locationPattern)
        {
            return _patchRepresentationAction.Execute(id, jObj, Common.Constants.SchemaUrns.User, locationPattern);
        }

        public Task<ApiActionResult> RemoveUser(string id)
        {
            return _deleteRepresentationAction.Execute(id);
        }

        public Task<ApiActionResult> GetUser(string id, string locationPattern)
        {
            return _getRepresentationAction.Execute(id, locationPattern, Common.Constants.SchemaUrns.User);
        }

        public Task<ApiActionResult> SearchUsers(JObject jObj, string locationPattern)
        {
            var searchParam = _searchParameterParser.ParseJson(jObj);
            return _getRepresentationsAction.Execute(Common.Constants.ResourceTypes.User, searchParam, locationPattern);
        }

        public Task<ApiActionResult> SearchUsers(IQueryCollection query, string locationPattern)
        {
            var searchParam = _searchParameterParser.ParseQuery(query);
            return _getRepresentationsAction.Execute(Common.Constants.ResourceTypes.User, searchParam, locationPattern);
        }
    }
}
