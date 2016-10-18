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

using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Results;
using SimpleIdentityServer.Scim.Core.Stores;
using System;
using System.Net;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IDeleteRepresentationAction
    {
        /// <summary>
        /// Remove the representation.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when the id is null or empty</exception>
        /// <param name="id">Representation's id</param>
        /// <returns>StatusCode with the content.</returns>
        ApiActionResult Execute(
            string id);
    }

    internal class DeleteRepresentationAction : IDeleteRepresentationAction
    {
        private readonly IRepresentationStore _representationStore;
        private readonly IApiResponseFactory _apiResponseFactory;

        public DeleteRepresentationAction(
            IRepresentationStore representationStore,
            IApiResponseFactory apiResponseFactory)
        {
            _representationStore = representationStore;
            _apiResponseFactory = apiResponseFactory;
        }

        /// <summary>
        /// Remove the representation.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when the id is null or empty</exception>
        /// <param name="id">Representation's id</param>
        /// <returns>StatusCode with the content.</returns>
        public ApiActionResult Execute(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            // 1. Get the representation
            var representation = _representationStore.GetRepresentation(id);

            // 2. If the representation doesn't exist then 404 is returned
            if (representation == null)
            {
                return _apiResponseFactory.CreateError(
                    HttpStatusCode.NotFound,
                    string.Format(ErrorMessages.TheResourceDoesntExist, id));
            }

            // 3. Remove the representation
            if (!_representationStore.RemoveRepresentation(representation))
            {
                return _apiResponseFactory.CreateError(HttpStatusCode.InternalServerError,
                    ErrorMessages.TheRepresentationCannotBeRemoved);
            }

            // 4. Returns the result.
            return _apiResponseFactory.CreateEmptyResult(HttpStatusCode.NoContent);
        }
    }
}
