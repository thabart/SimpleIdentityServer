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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Client.Builders
{
    public class AddRequestBuilder
    {
        private readonly Func<JObject, Task<JObject>> _callback;
        private JObject _obj;

        public AddRequestBuilder(string schema, Func<JObject, Task<JObject>> callback)
        {
            if (string.IsNullOrWhiteSpace(schema))
            {
                throw new ArgumentNullException(nameof(schema));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            _callback = callback;
            Initialize(new string[] { schema });
        }

        public AddRequestBuilder(IEnumerable<string> schemas, Func<JObject, Task<JObject>> callback)
        {
            if (schemas == null)
            {
                throw new ArgumentNullException(nameof(schemas));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            _callback = callback;
            Initialize(schemas);
        }

        public AddRequestBuilder SetCommonAttributes(string externalId)
        {
            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new ArgumentNullException(nameof(externalId));
            }

            _obj[Common.Constants.IdentifiedScimResourceNames.ExternalId] = externalId;
            return this;
        }

        public async Task<JObject> Execute()
        {
            return await _callback(_obj);
        }

        private void Initialize(IEnumerable<string> schemas)
        {
            var arr = new JArray(schemas);
            _obj = new JObject();
            _obj[Common.Constants.ScimResourceNames.Schemas] = arr;
        }
    }
}
