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
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SimpleIdentityServer.IdentityServer.EF.Repositories
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
            var user = _context.Users.Include(u => u.Claims)
                .FirstOrDefault(u => u.Subject == subject);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return true;
        }

        public List<ResourceOwner> GetAll()
        {
            var users = _context.Users.ToList();
            return users.Select(u => u.ToDomain()).ToList();
        }

        public ResourceOwner GetBySubject(string subject)
        {
            var user = _context.Users.Include(u => u.Claims).FirstOrDefault(r => r.Subject == subject);
            if (user == null)
            {
                return null;
            }

            return user.ToDomain();
        }

        public ResourceOwner GetResourceOwnerByCredentials(string userName, string hashedPassword)
        {            
            var user = _context.Users.Include(u => u.Claims).FirstOrDefault(r => r.Username == userName && r.Password == hashedPassword);
            if (user == null)
            {
                return null;
            }
            
            return user.ToDomain();
        }

        public bool Insert(ResourceOwner resourceOwner)
        {
            try
            {
                var user = resourceOwner.ToEntity();
                _context.Users.Add(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _managerEventSource.Failure(ex);
                return false;
            }

            return true;
        }

        public bool Update(ResourceOwner resourceOwner)
        {
            var user = resourceOwner.ToEntity();
            var record = _context.Users
               .Include(r => r.Claims)
               .FirstOrDefault(r => r.Subject == user.Subject);
            if (record == null)
            {
                return false;
            }

            record.Username = user.Username;
            record.Password = user.Password;
            record.IsLocalAccount = resourceOwner.IsLocalAccount;
            record.Claims = user.Claims;
            _context.SaveChanges();
            return true;
        }

        #endregion
    }
}
