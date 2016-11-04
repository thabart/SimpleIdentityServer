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
using System.Collections.Generic;
using System.Net.Http;

namespace SimpleIdentityServer.Scim.Core.Models
{
    public class BulkOperationResult
    {
        /// <summary>
        /// HTTP method of the current operation.
        /// </summary>
        public HttpMethod Method { get; set; }
        /// <summary>
        /// Transient identifier of a newly created resource.
        /// Unique and created by the client. 
        /// REQUIRED when method is "POST"
        /// </summary>
        public string BulkId { get; set; }
        /// <summary>
        /// Current resource version.
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Name of the schema.
        /// </summary>
        public string SchemaName { get; set; }
        /// <summary>
        /// Resource identifier.
        /// </summary>
        public string ResourceId { get; set; }
        /// <summary>
        /// Resource data as it would appear for a single SCIM POST, PUT etc ...
        /// </summary>
        public JToken Data { get; set; }
    }

    public class BulkResult
    {
        /// <summary>
        /// Number of errors that the service provider will accept before the operation is terminated
        /// and an error response is returned.
        /// </summary>
        public int? FailOnErrors { get; set; }
        /// <summary>
        /// Operations within a bulk job.
        /// </summary>
        public IEnumerable<BulkOperationResult> Operations { get; set; }
    }
}
