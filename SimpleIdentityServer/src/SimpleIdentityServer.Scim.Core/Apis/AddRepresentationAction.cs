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
using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Stores;
using System;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IAddRepresentationAction
    {
        bool Execute(JObject jObj, string id);
    }

    internal class AddRepresentationAction : IAddRepresentationAction
    {
        private readonly IRequestParser _requestParser;
        private readonly IRepresentationStore _representationStore;

        public AddRepresentationAction(IRequestParser requestParser, IRepresentationStore representationStore)
        {
            _requestParser = requestParser;
            _representationStore = representationStore;
        }

        public bool Execute(JObject jObj, string id)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            // 1. Parse the request
            var result = _requestParser.Parse(jObj, id);
            if (result == null)
            {
                throw new InvalidOperationException(ErrorMessages.TheRequestCannotBeParsedForSomeReason);
            }

            // 2. Save the request
            _representationStore.AddRepresentation(result);

            // 3. Transform the representation into response

            return true;
        }
    }
}
