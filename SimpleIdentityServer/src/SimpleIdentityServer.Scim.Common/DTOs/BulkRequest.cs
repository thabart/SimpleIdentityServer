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
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Scim.Common.DTOs
{
    [DataContract]
    public class BulkOperationRequest
    {
        /// <summary>
        /// HTTP method of the current operation.
        /// </summary>
        [DataMember(Name = Constants.BulkOperationRequestNames.Method)]
        public string Method { get; set; }
        /// <summary>
        /// Transient identifier of a newly created resource.
        /// Unique and created by the client. 
        /// REQUIRED when method is "POST"
        /// </summary>
        [DataMember(Name = Constants.BulkOperationRequestNames.BulkId)]
        public string BulkId { get; set; }
        /// <summary>
        /// Current resource version.
        /// </summary>
        [DataMember(Name = Constants.BulkOperationRequestNames.Version)]
        public string Version { get; set; }
        [DataMember(Name = Constants.BulkOperationRequestNames.Path)]
        /// <summary>
        /// Resource's relative path to the SCIM service provider's root.
        /// POST : "/Users" or "/Groups"
        /// OTHERS : "/Users/<id>" or "/Groups/<id>"
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Resource data as it would appear for a single SCIM POST, PUT etc ...
        /// </summary>
        [DataMember(Name = Constants.BulkOperationRequestNames.Data)]
        public JToken Data { get; set; }
    }

    [DataContract]
    public class BulkRequest
    {
        [DataMember(Name = Constants.ScimResourceNames.Schemas)]
        public IEnumerable<string> Schemas { get; set; }
        /// <summary>
        /// Number of errors that the service provider will accept before the operation is terminated
        /// and an error response is returned.
        /// </summary>
        [DataMember(Name = Constants.BulkRequestNames.FailOnErrors)]
        public int? FailOnErrors { get; set; }
        /// <summary>
        /// Operations within a bulk job.
        /// </summary>
        [DataMember(Name = Constants.BulkRequestNames.Operations)]
        public IEnumerable<BulkOperationRequest> Operations { get; set; }
    }
}
