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
                    Password = "password"
                }
            };
        } 
    }
}