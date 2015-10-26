using System;

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;

using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;

using System.Linq;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeResourceOwnerRepository : IResourceOwnerRepository
    {
        public ResourceOwner GetBySubject(string subject)
        {
            var record = FakeDataSource.Instance().ResourceOwners.SingleOrDefault(r => r.Id == subject);
            if (record == null)
            {
                return null;
            }

            return new ResourceOwner
            {
                Id = record.Id,
                UserName = record.UserName,
                Password = record.Password
            };
        }

        public ResourceOwner GetResourceOwnerByCredentials(string userName, string hashedPassword)
        {
            var record = FakeDataSource.Instance().ResourceOwners.SingleOrDefault(r => r.UserName == userName && r.Password == hashedPassword);
            if (record == null)
            {
                return null;
            }

            return new ResourceOwner
            {
                Id = record.Id,
                UserName = record.UserName,
                Password = record.Password
            };
        }

        public bool Insert(ResourceOwner resourceOwner)
        {
            FakeDataSource.Instance().ResourceOwners.Add(new MODELS.ResourceOwner
            {
                Id = resourceOwner.Id,
                UserName = resourceOwner.UserName,
                Password = resourceOwner.Password
            });

            return true;
        }
    }
}
