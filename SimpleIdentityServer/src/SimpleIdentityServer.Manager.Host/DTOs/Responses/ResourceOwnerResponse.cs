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

namespace SimpleIdentityServer.Manager.Host.DTOs.Responses
{
    [DataContract]
    public class ResourceOwnerResponse
    {       
        [DataMember(Name = Constants.ResourceOwnerResponseNames.Id)]
        public string Id { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.Name)]
        public string Name { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.GivenName)]
        public string GivenName { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.FamilyName)]
        public string FamilyName { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.MiddleName)]
        public string MiddleName { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.NickName)]
        public string NickName { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.PreferredUserName)]
        public string PreferredUserName { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.Profile)]
        public string Profile { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.Picture)]
        public string Picture { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.WebSite)]
        public string WebSite { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.Email)]
        public string Email { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.EmailVerified)]
        public bool EmailVerified { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.Gender)]
        public string Gender { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.BirthDate)]
        public string BirthDate { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.ZoneInfo)]
        public string ZoneInfo { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.Locale)]
        public string Locale { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.PhoneNumberVerified)]
        public bool PhoneNumberVerified { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.UpdatedAt)]
        public double UpdatedAt { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.Password)]
        public string Password { get; set; }

        [DataMember(Name = Constants.ResourceOwnerResponseNames.Roles)]
        public List<string> Roles { get; set; }
    }
}
