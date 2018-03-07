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

using System.Runtime.Serialization;

namespace SimpleIdentityServer.UmaManager.Client.DTOs.Requests
{
    public enum AuthorizationPolicyFilters
    {
        All,
        Root,
        NotRoot
    }

    [DataContract]
    public class SearchResourceRequest
    {
        [DataMember(Name = Constants.SearchResourceRequestNames.Url)]
        public string Url { get; set; }

        [DataMember(Name = Constants.SearchResourceRequestNames.ResourceId)]
        public string ResourceId { get; set; }

        [DataMember(Name = Constants.SearchResourceRequestNames.IsExactUrl)]
        public bool IsExactUrl { get; set; }

        [DataMember(Name = Constants.SearchResourceRequestNames.AuthorizationPolicy)]
        public string AuthorizationPolicy { get; set; }

        [DataMember(Name = Constants.SearchResourceRequestNames.AuthorizationPolicyFilter)]
        public AuthorizationPolicyFilters AuthorizationPolicyFilter { get; set; }
    }
}
