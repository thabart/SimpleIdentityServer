using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Models
{
    public class Address
    {
        /// <summary>
        /// Gets or sets the full mailing address formatted for display or use on a mailing label.
        /// </summary>
        public string Formatted { get; set; }

        /// <summary>
        /// Gets or sets the full street address component.
        /// </summary>
        public string StreetAddress { get; set; }

        /// <summary>
        /// Gets or sets the city or locality component.
        /// </summary>
        public string Locality { get; set; }

        /// <summary>
        /// Gets or sets the state, province, prefecture or region component.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the zip or postal code component.
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the country name component.
        /// </summary>
        public string Country { get; set; }
    }
}
