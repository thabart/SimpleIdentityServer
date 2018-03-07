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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Scim.Common.DTOs
{
    /// <summary>
    /// Parameters are listed here : https://tools.ietf.org/html/rfc7643#section-3.1
    /// </summary>
    [DataContract]
    public class Meta
    {
        [DataMember(Name = Constants.MetaResponseNames.ResourceType)]
        public string ResourceType { get; set; }

        [DataMember(Name = Constants.MetaResponseNames.Created)]
        public DateTime Created { get; set; }
        
        [DataMember(Name = Constants.MetaResponseNames.LastModified)]
        public DateTime LastModified { get; set; }

        [DataMember(Name = Constants.MetaResponseNames.Location)]
        public string Location { get; set; }

        [DataMember(Name = Constants.MetaResponseNames.Version)]
        public string Version { get; set; }
    }

    /// <summary>
    /// Parameters are listed here : https://tools.ietf.org/html/rfc7643#section-3
    /// </summary>
    [DataContract]
    public class ScimResource
    {
        [DataMember(Name = Constants.ScimResourceNames.Schemas)]
        public IEnumerable<string> Schemas { get; set; }

        [DataMember(Name = Constants.ScimResourceNames.Meta)]
        public Meta Meta { get; set; }
    }
}
