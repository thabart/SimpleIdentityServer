using System;

namespace SimpleIdentityServer.DataAccess.Fake.Models
{
    public class AuthorizationCode
    {
        public string Value { get; set; }

        public DateTime CreateDateTime { get; set; }

        public Consent Consent { get; set; }
    }
}
