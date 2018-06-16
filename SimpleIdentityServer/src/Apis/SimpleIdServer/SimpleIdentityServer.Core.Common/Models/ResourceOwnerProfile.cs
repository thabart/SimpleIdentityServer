using System;

namespace SimpleIdentityServer.Core.Common.Models
{
    public class ResourceOwnerProfile
    {
        /// <summary>
        /// Gets or sets subject.
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// Get or sets the name of the issuer.
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// Gets or sets the resource owner id.
        /// </summary>
        public string ResourceOwnerId { get; set; }
        /// <summary>
        /// Gets or sets the create datetime.
        /// </summary>
        public DateTime CreateDateTime { get; set; }
        /// <summary>
        /// Gets or sets the update datetime.
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}
