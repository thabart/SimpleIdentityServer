using System;
using SimpleIdentityServer.Core.Repositories;
using System.Collections.Generic;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeJwtBearerClientRepository : IJwtBearerClientRepository
    {
        private readonly List<string> _jwtBearerClients;

        public FakeJwtBearerClientRepository()
        {
            _jwtBearerClients = new List<string>();
        }

        public bool Exist(string id)
        {
            return _jwtBearerClients.Contains(id);
        }

        public bool Insert(string id)
        {
            _jwtBearerClients.Add(id);
            return true;
        }
    }
}
