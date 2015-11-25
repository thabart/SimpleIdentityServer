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
