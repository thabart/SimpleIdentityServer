using System;

namespace SimpleIdentityServer.EF.Models
{
    public class Profile
    {
        public string Subject { get; set; }
        public string Issuer { get; set; }
        public string ResourceOwnerId { get; set; }
        public DateTime UpdateDateTime { get; set; }
        public DateTime CreateDateTime { get; set; }
        public virtual ResourceOwner ResourceOwner { get; set; }
    }
}
