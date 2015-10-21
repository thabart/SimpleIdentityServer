using System.Collections.Generic;

using SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.Api.Configuration
{
    public static class ResourceOwners
    {
        public static List<ResourceOwner> Get()
        {
            return new List<ResourceOwner>
            {
                new ResourceOwner
                {
                    Id = "administrator",
                    Password = "5E884898DA28047151D0E56F8DC6292773603D0D6AABBDD62A11EF721D1542D8"
                }
            };
        } 
    }
}