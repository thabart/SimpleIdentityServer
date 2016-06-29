using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;

using System.Linq;
using SimpleIdentityServer.DataAccess.Fake.Extensions;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeResourceOwnerRepository : IResourceOwnerRepository
    {
        private readonly FakeDataSource _fakeDataSource;
        
        public FakeResourceOwnerRepository(FakeDataSource fakeDataSource) 
        {
            _fakeDataSource = fakeDataSource;
        }

        public List<ResourceOwner> GetAll()
        {
            return null;
        }

        public ResourceOwner GetBySubject(string subject)
        {
            var record = _fakeDataSource.ResourceOwners.SingleOrDefault(r => r.Id == subject);
            if (record == null)
            {
                return null;
            }

            return record.ToBusiness();
        }

        public ResourceOwner GetResourceOwnerByCredentials(string userName, string hashedPassword)
        {
            var record = _fakeDataSource.ResourceOwners.SingleOrDefault(r => r.Name == userName && r.Password == hashedPassword);
            if (record == null)
            {
                return null;
            }

            return record.ToBusiness();
        }

        public bool Insert(ResourceOwner resourceOwner)
        {
            _fakeDataSource.ResourceOwners.Add(resourceOwner.ToFake());
            return true;
        }

        public bool Update(ResourceOwner resourceOwner)
        {
            return true;
        }
    }
}
