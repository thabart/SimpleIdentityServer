using System;

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;

using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;

using System.Linq;
using SimpleIdentityServer.DataAccess.Fake.Extensions;

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

            return record.ToBusiness();
        }

        public ResourceOwner GetResourceOwnerByCredentials(string userName, string hashedPassword)
        {
            var record = FakeDataSource.Instance().ResourceOwners.SingleOrDefault(r => r.UserName == userName && r.Password == hashedPassword);
            if (record == null)
            {
                return null;
            }

            return record.ToBusiness();
        }

        public bool Insert(ResourceOwner resourceOwner)
        {
            FakeDataSource.Instance().ResourceOwners.Add(resourceOwner.ToFake());
            return true;
        }
    }
}
