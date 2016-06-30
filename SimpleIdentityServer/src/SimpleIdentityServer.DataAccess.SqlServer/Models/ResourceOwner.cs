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

namespace SimpleIdentityServer.DataAccess.SqlServer.Models
{
    public class ResourceOwner
    {        
        /// <summary>
        /// Get or sets the subject-identifier for the End-User at the issuer.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the End-User's full name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name or first-name of the End-User.
        /// </summary>
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets the surname or last name of the end-user.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets the middle name of the end-user.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets casual name of the end-user that may or may not be the same as the given name.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the shorthand name by which the end-user whishes to be referred to at the RP.
        /// </summary>
        public string PreferredUserName { get; set; }

        /// <summary>
        /// Gets or sets the End-User's profile page.
        /// </summary>
        public string Profile { get; set; }

        /// <summary>
        /// Gets or sets the URL of the end-user's profile picture.
        /// </summary>
        public string Picture { get; set; }

        /// <summary>
        /// Gets or sets the URL of the end-user's web page or blog.
        /// </summary>
        public string WebSite { get; set; }

        /// <summary>
        /// Gets or sets the End-User's preferred email-adress.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets true if the End-User's email has been verified; otherwise false.
        /// </summary>
        public bool EmailVerified { get; set; }

        /// <summary>
        /// Gets or sets the End-User's gender.
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the End-User's birthdate, represented as an ISO 8601:2004 YYYY-MM-DD format.
        /// </summary>
        public string BirthDate { get; set; }

        /// <summary>
        /// Gets or sets the End-User's zone info. For example : Europe/Paris, America/Los_Angeles.
        /// </summary>
        public string ZoneInfo { get; set; }

        /// <summary>
        /// Gets or sets the End-User's locale, represented as a BCP47[RFC5646] language tag for example : en-US or fr-CA.
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// Gets or sets the End-User's preferred telephone number.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets true if the End-User's phone number has been verified; otherwise false.
        /// </summary>
        public bool PhoneNumberVerified { get; set; }

        /// <summary>
        /// Gets or sets the time the end-user's information was last updated.
        /// Its value is a JSON number representing the number of seconds.
        /// </summary>
        public double UpdatedAt { get; set; }

        public string Password { get; set; }

        /// <summary>
        /// Gets or sets local account
        /// </summary>
        public bool IsLocalAccount { get; set; }
        
        /// <summary>
        /// Gets or sets the End-User's preferred postal address. The value is a JSON structure containing some or all the members defined in 
        /// http://openid.net/specs/openid-connect-core-1_0.html#AddressClaim
        /// </summary>
        public Address Address { get; set; }

        public virtual List<ResourceOwnerRole> ResourceOwnerRoles { get; set; }

        /// <summary>
        /// Gets or sets the list of consents
        /// </summary>
        public virtual List<Consent> Consents { get; set; } 
    }
}
