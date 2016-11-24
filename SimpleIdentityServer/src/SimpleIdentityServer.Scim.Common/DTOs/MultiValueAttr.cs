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

using SimpleIdentityServer.Scim.Common;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Scim.Common.DTOs
{
    [DataContract]
    public class MultiValueAttr
    {
        [DataMember(Name = Constants.MultiValueAttributeNames.Type)]
        public string Type { get; set; }

        [DataMember(Name = Constants.MultiValueAttributeNames.Primary)]
        public bool Primary { get; set; }

        [DataMember(Name = Constants.MultiValueAttributeNames.Display)]
        public string Display { get; set; }

        [DataMember(Name = Constants.MultiValueAttributeNames.Value)]
        public string Value { get; set; }

        [DataMember(Name = Constants.MultiValueAttributeNames.Ref)]
        public string Ref { get; set; }
    }
}
