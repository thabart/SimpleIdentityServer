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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleBus.Core;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Results;
using SimpleIdentityServer.Scim.Events;
using System;
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
        private readonly IEventPublisher _eventPublisher;

        public GroupsAction(
            IAddRepresentationAction addRepresentationAction,
            IGetRepresentationAction getRepresentationAction,
            IDeleteRepresentationAction deleteRepresentationAction,
            IUpdateRepresentationAction updateRepresentationAction,
            IPatchRepresentationAction patchRepresentationAction,
            ISearchParameterParser searchParameterParser,
            IGetRepresentationsAction getRepresentationsAction,
            IEventPublisher eventPublisher)
        {
            _addRepresentationAction = addRepresentationAction;
            _getRepresentationAction = getRepresentationAction;
            _deleteRepresentationAction = deleteRepresentationAction;
            _updateRepresentationAction = updateRepresentationAction;
            _patchRepresentationAction = patchRepresentationAction;
            _searchParameterParser = searchParameterParser;
            _getRepresentationsAction = getRepresentationsAction;
            _eventPublisher = eventPublisher;
        }

        public async Task<ApiActionResult> AddGroup(JObject jObj, string locationPattern)
        {
            var processId = Guid.NewGuid().ToString();
            try
            {
                _eventPublisher.Publish(new AddGroupReceived(Guid.NewGuid().ToString(), processId, jObj.ToString(), 0));
                var result = await _addRepresentationAction.Execute(jObj, locationPattern, Common.Constants.SchemaUrns.Group, Common.Constants.ResourceTypes.Group);
                _eventPublisher.Publish(new AddGroupFinished(Guid.NewGuid().ToString(), processId, JsonConvert.SerializeObject(result).ToString(), 1));
                return result;
            }
            catch (Exception ex)
            {
                _eventPublisher.Publish(new ScimErrorReceived(Guid.NewGuid().ToString(), processId, ex.Message, 1));
                throw;
            }
        }

        public Task<ApiActionResult> GetGroup(string id, string locationPattern, IQueryCollection query)
        {
            var searchParameter = _searchParameterParser.ParseQuery(query);
            return _getRepresentationAction.Execute(id, locationPattern, Common.Constants.SchemaUrns.Group);
        }

        public async Task<ApiActionResult> RemoveGroup(string id)
        {
            var processId = Guid.NewGuid().ToString();
            try
            {
                var jObj = new JObject();
                jObj.Add("id", id);
                _eventPublisher.Publish(new RemoveGroupReceived(Guid.NewGuid().ToString(), processId, jObj.ToString(), 0));
                var result = await _deleteRepresentationAction.Execute(id);
                _eventPublisher.Publish(new RemoveGroupFinished(Guid.NewGuid().ToString(), processId, JsonConvert.SerializeObject(result).ToString(), 1));
                return result;
            }
            catch(Exception ex)
            {
                _eventPublisher.Publish(new ScimErrorReceived(Guid.NewGuid().ToString(), processId, ex.Message, 1));
                throw;
            }
        }

        public async Task<ApiActionResult> UpdateGroup(string id, JObject jObj, string locationPattern)
        {
            var processId = Guid.NewGuid().ToString();
            try
            {
                _eventPublisher.Publish(new RemoveGroupReceived(Guid.NewGuid().ToString(), processId, jObj.ToString(), 0));
                var result = await _updateRepresentationAction.Execute(id, jObj, Common.Constants.SchemaUrns.Group, locationPattern, Common.Constants.ResourceTypes.Group);
                _eventPublisher.Publish(new RemoveGroupFinished(Guid.NewGuid().ToString(), processId, JsonConvert.SerializeObject(result).ToString(), 1));
                return result;
            }
            catch(Exception ex)
            {
                _eventPublisher.Publish(new ScimErrorReceived(Guid.NewGuid().ToString(), processId, ex.Message, 1));
                throw;
            }
        }

        public async Task<ApiActionResult> PatchGroup(string id, JObject jObj, string locationPattern)
        {
            var processId = Guid.NewGuid().ToString();
            try
            {
                _eventPublisher.Publish(new PatchGroupReceived(Guid.NewGuid().ToString(), processId, jObj.ToString(), 0));
                var result = await _patchRepresentationAction.Execute(id, jObj, Common.Constants.SchemaUrns.Group, locationPattern);
                _eventPublisher.Publish(new PatchGroupFinished(Guid.NewGuid().ToString(), processId, JsonConvert.SerializeObject(result).ToString(), 1));
                return result;
            }
            catch(Exception ex)
            {
                _eventPublisher.Publish(new ScimErrorReceived(Guid.NewGuid().ToString(), processId, ex.Message, 1));
                throw;
            }
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
