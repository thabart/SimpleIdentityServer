using System;

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;

using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;

using System.Linq;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeResourceOwnerRepository : IResourceOwnerRepository
    {
        public ResourceOwner GetResourceOwnerByCredentials(string userName, string hashedPassword)
        {
            var record = FakeDataSource.Instance().ResourceOwners.SingleOrDefault(r => r.Id == userName && r.Password == hashedPassword);
            return new ResourceOwner
            {
                Id = record.Id,
                Password = record.Password
            };
        }

        public bool Insert(ResourceOwner resourceOwner)
        {
            FakeDataSource.Instance().ResourceOwners.Add(new MODELS.ResourceOwner
            {
                Id = resourceOwner.Id,
                Password = resourceOwner.Password
            });

            return true;
        }
    }
}
