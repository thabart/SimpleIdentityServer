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

using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Scim.Startup.DTOs.Responses
{
    [DataContract]
    internal class NameResponse
    {
        [DataMember(Name = Constants.NameResponseNames.Formatted)]
        public string Formatted { get; set; }
        [DataMember(Name = Constants.NameResponseNames.FamilyName)]
        public string FamilyName { get; set; }
        [DataMember(Name = Constants.NameResponseNames.GivenName)]
        public string GivenName { get; set; }
        [DataMember(Name = Constants.NameResponseNames.MiddleName)]
        public string MiddleName { get; set; }
        [DataMember(Name = Constants.NameResponseNames.HonorificPrefix)]
        public string HonorificPrefix { get; set; }
        [DataMember(Name = Constants.NameResponseNames.HonorificSuffix)]
        public string HonorificSuffix { get; set; }
    }

    [DataContract]
    internal class AddressResponse : MultiValueAttrResponse
    {
        [DataMember(Name = Constants.AddressResponseNames.Formatted)]
        public string Formatted { get; set; }
        [DataMember(Name = Constants.AddressResponseNames.StreetAddress)]
        public string StreetAddress { get; set; }
        [DataMember(Name = Constants.AddressResponseNames.Locality)]
        public string Locality { get; set; }
        [DataMember(Name = Constants.AddressResponseNames.Region)]
        public string Region { get; set; }
        [DataMember(Name = Constants.AddressResponseNames.PostalCode)]
        public string PostalCode { get; set; }
        [DataMember(Name = Constants.AddressResponseNames.Country)]
        public string Country { get; set; }
    }

    [DataContract]
    internal class UserResourceResponse : IdentifiedScimResourceResponse
    {
        [DataMember(Name = Constants.UserResourceResponseNames.UserName)]
        public string UserName { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.Name)]
        public NameResponse Name { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.DisplayName)]
        public string DisplayName { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.NickName)]
        public string NickName { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.ProfileUrl)]
        public string ProfileUrl { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.Title)]
        public string Title { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.UserType)]
        public string UserType { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.PreferredLanguage)]
        public string PreferredLanguage { get; set; }
        /// <summary>
        /// Read the RFC : https://tools.ietf.org/html/rfc5646
        /// </summary>
        [DataMember(Name = Constants.UserResourceResponseNames.Locale)]
        public string Locale { get; set; }
        /// <summary>
        /// Read the RFC : https://tools.ietf.org/html/rfc6557
        /// </summary>
        [DataMember(Name = Constants.UserResourceResponseNames.Timezone)]
        public string Timezone { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.Active)]
        public bool Active { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.Password)]
        public string Password { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.Emails)]
        public IEnumerable<MultiValueAttrResponse> Emails { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.Phones)]
        public IEnumerable<MultiValueAttrResponse> Phones { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.Ims)]
        public IEnumerable<MultiValueAttrResponse> Ims { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.Photos)]
        public IEnumerable<MultiValueAttrResponse>  Photos { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.Addresses)]
        public IEnumerable<AddressResponse> Addresses { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.Groups)]
        public IEnumerable<AddressResponse> Groups { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.Entitlements)]
        public IEnumerable<MultiValueAttrResponse> Entitlements { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.Roles)]
        public IEnumerable<MultiValueAttrResponse> Roles { get; set; }
        [DataMember(Name = Constants.UserResourceResponseNames.X509Certificates)]
        public IEnumerable<MultiValueAttrResponse> X509Certificates { get; set; }
    }
}
