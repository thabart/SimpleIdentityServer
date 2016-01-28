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

namespace SimpleIdentityServer.Core.Models
{
    [DataContract]
    public class Address
    {
        /// <summary>
        /// Gets or sets the full mailing address formatted for display or use on a mailing label.
        /// </summary>
        [DataMember(Name = "formatted")]
        public string Formatted { get; set; }

        /// <summary>
        /// Gets or sets the full street address component.
        /// </summary>
        [DataMember(Name = "street_address")]
        public string StreetAddress { get; set; }

        /// <summary>
        /// Gets or sets the city or locality component.
        /// </summary>
        [DataMember(Name = "locality")]
        public string Locality { get; set; }

        /// <summary>
        /// Gets or sets the state, province, prefecture or region component.
        /// </summary>
        [DataMember(Name = "region")]
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the zip or postal code component.
        /// </summary>
        [DataMember(Name = "postal_code")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the country name component.
        /// </summary>
        [DataMember(Name = "country")]
        public string Country { get; set; }
    }
}
