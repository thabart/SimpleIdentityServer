#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.IdentityServer.EF.DbContexts;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.IdentityServer.EF
{
    public class ResourceOwnerRepository : IResourceOwnerRepository
    {
        #region Fields

        private readonly UserDbContext _context;

        private readonly IManagerEventSource _managerEventSource;

        #endregion
        
        #region Constructor

        public ResourceOwnerRepository(
            UserDbContext context,
            IManagerEventSource managerEventSource)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (managerEventSource == null)
            {
                throw new ArgumentNullException(nameof(managerEventSource));
            }

            _context = context;
            _managerEventSource = managerEventSource;
        }

        #endregion

        #region Public methods

        public bool Delete(string subject)
        {
            throw new NotImplementedException();
        }

        public List<ResourceOwner> GetAll()
        {
            throw new NotImplementedException();
        }

        public ResourceOwner GetBySubject(string subject)
        {
            throw new NotImplementedException();
        }

        public ResourceOwner GetResourceOwnerByCredentials(string userName, string hashedPassword)
        {
            throw new NotImplementedException();
        }

        public bool Insert(ResourceOwner resourceOwner)
        {
            throw new NotImplementedException();
        }

        public bool Update(ResourceOwner resourceOwner)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
