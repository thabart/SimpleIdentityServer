using System;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeGrantedTokenRepository : IGrantedTokenRepository
    {
        public bool Insert(GrantedToken grantedToken)
        {
            return true;
        }
    }
}
