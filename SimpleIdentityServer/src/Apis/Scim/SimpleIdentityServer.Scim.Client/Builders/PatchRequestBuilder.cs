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
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Client.Builders
{
    public enum PatchOperations
    {
        add,
        remove,
        replace
    }

    public class PatchOperation
    {
        public PatchOperations Type { get; set; }
        public string Path { get; set; }
        public JToken Value { get; set; }
    }

    public class PatchOperationBuilder
    {
        private readonly PatchOperation _patchOperation;

        public PatchOperationBuilder()
        {
            _patchOperation = new PatchOperation();
        }

        public PatchOperationBuilder SetType(PatchOperations type)
        {
            _patchOperation.Type = type;
            return this;
        }

        public PatchOperationBuilder SetPath(string path)
        {
            // TODO : Check the path.
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            _patchOperation.Path = path;
            return this;
        }
        
        public PatchOperationBuilder SetContent(JToken obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            _patchOperation.Value = obj;
            return this;
        }

        public PatchOperation Build()
        {
            if (_patchOperation.Type == PatchOperations.remove)
            {
                if (string.IsNullOrWhiteSpace(_patchOperation.Path))
                {
                    throw new InvalidOperationException("the path should be specified");
                }

                if (_patchOperation.Value != null)
                {
                    throw new InvalidOperationException("the content shouldn't be specified");
                }
            }

            if ((_patchOperation.Type == PatchOperations.add || _patchOperation.Type == PatchOperations.replace ) && _patchOperation.Value == null)
            {
                throw new InvalidOperationException("the content should be specified");
            }

            return _patchOperation;
        }
    }

    public class PatchRequestBuilder
    {
        private readonly Func<JObject, Task<ScimResponse>> _callback;
        private IList<PatchOperation> _operations;
        private JObject _obj;

        public PatchRequestBuilder(Func<JObject, Task<ScimResponse>> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            _callback = callback;
            _operations = new List<PatchOperation>();
            Initialize(new string[] { Common.Constants.Messages.PatchOp });
        }

        public PatchRequestBuilder AddOperation(PatchOperation operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            _operations.Add(operation);
            return this;
        }

        public async Task<ScimResponse> Execute()
        {
            if (!_operations.Any())
            {
                throw new InvalidOperationException("at least one operation should be inserted");
            }

            var arr = new JArray();
            foreach(var operation in _operations)
            {
                var obj = new JObject();
                obj.Add(new JProperty(Common.Constants.PatchOperationRequestNames.Operation, Enum.GetName(typeof(PatchOperations), operation.Type)));
                if (!string.IsNullOrWhiteSpace(operation.Path))
                {
                    obj.Add(new JProperty(Common.Constants.PatchOperationRequestNames.Path, operation.Path));
                }

                if (operation.Value != null)
                {
                    obj.Add(new JProperty(Common.Constants.PatchOperationRequestNames.Value, operation.Value));
                }

                arr.Add(obj);
            }

            _obj.Add(new JProperty(Common.Constants.PatchOperationsRequestNames.Operations, arr));
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
