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

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Uma.Host.DTOs.Responses
{
    [DataContract]
    public class ResourceSetResponse
    {
        [DataMember(Name = Constants.ResourceSetResponseNames.Id)]
        public string Id { get; set; }

        [DataMember(Name = Constants.ResourceSetResponseNames.Name)]
        public string Name { get; set; }

        [DataMember(Name = Constants.ResourceSetResponseNames.Uri)]
        public string Uri { get; set; }

        [DataMember(Name = Constants.ResourceSetResponseNames.Type)]
        public string Type { get; set; }

        [DataMember(Name = Constants.ResourceSetResponseNames.Scopes)]
        public List<string> Scopes { get; set; }

        [DataMember(Name = Constants.ResourceSetResponseNames.IconUri)]
        public string IconUri { get; set; }
    }
}
