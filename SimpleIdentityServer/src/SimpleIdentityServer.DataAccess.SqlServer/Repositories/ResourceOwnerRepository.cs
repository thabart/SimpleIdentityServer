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

using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using SimpleIdentityServer.DataAccess.SqlServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Domains = SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class ResourceOwnerRepository : IResourceOwnerRepository
    {
        #region Fields

        private readonly SimpleIdentityServerContext _context;

        #endregion

        #region Constructor

        public ResourceOwnerRepository(SimpleIdentityServerContext context)
        {
            _context = context;
        }

        #endregion

        #region Public methods

        public Domains.ResourceOwner GetResourceOwnerByCredentials(
            string userName,
            string hashedPassword)
        {
            // 1. Fetch the user information & returns null if he doesn't exist
            var user = _context.ResourceOwners.FirstOrDefault(r => r.Name == userName && r.Password == hashedPassword);
            if (user == null)
            {
                return null;
            }

            // 2. Set the resource owner's roles
            user.ResourceOwnerRoles = _context.ResourceOwnerRoles
                .Include(r => r.Role)
                .Where(r => r.ResourceOwnerId == user.Id)
                .ToList();
            return user.ToDomain();
        }

        public bool Insert(Domains.ResourceOwner resourceOwner)
        {
            try
            {
                // 1. Add all the information
                var user = new Models.ResourceOwner
                {
                    Name = resourceOwner.Name,
                    BirthDate = resourceOwner.BirthDate,
                    Email = resourceOwner.Email,
                    EmailVerified = resourceOwner.EmailVerified,
                    FamilyName = resourceOwner.FamilyName,
                    Gender = resourceOwner.Gender,
                    GivenName = resourceOwner.GivenName,
                    Locale = resourceOwner.Locale,
                    MiddleName = resourceOwner.MiddleName,
                    NickName = resourceOwner.NickName,
                    Password = resourceOwner.Password,
                    PhoneNumber = resourceOwner.PhoneNumber,
                    PhoneNumberVerified = resourceOwner.PhoneNumberVerified,
                    Picture = resourceOwner.Picture,
                    PreferredUserName = resourceOwner.PreferredUserName,
                    Profile = resourceOwner.Profile,
                    UpdatedAt = resourceOwner.UpdatedAt,
                    WebSite = resourceOwner.WebSite,
                    ZoneInfo = resourceOwner.ZoneInfo,
                    Id = resourceOwner.Id,
                    ResourceOwnerRoles = new List<Models.ResourceOwnerRole>()
                };

                // 2. Add information about the user's address
                if (resourceOwner.Address != null)
                {
                    user.Address = new Models.Address
                    {
                        Country = user.Address.Country,
                        Formatted = user.Address.Formatted,
                        Locality = user.Address.Locality,
                        PostalCode = user.Address.PostalCode,
                        Region = user.Address.Region,
                        StreetAddress = user.Address.StreetAddress
                    };
                }

                // 3. Add all the roles
                if (resourceOwner.Roles != null &&
                    resourceOwner.Roles.Any())
                {
                    resourceOwner.Roles.ForEach(r =>
                    {
                        var resourceOwnerRole = new Models.ResourceOwnerRole
                        {
                            RoleName = r
                        };

                        user.ResourceOwnerRoles.Add(resourceOwnerRole);
                    });
                }

                _context.ResourceOwners.Add(user);
                _context.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public Domains.ResourceOwner GetBySubject(string subject)
        {
            // 1. Fetch the user information & return null if he doesn't exist
            var user = _context.ResourceOwners.FirstOrDefault(r => r.Id == subject);
            if (user == null)
            {
                return null;
            }
            
            // 2. Set the resource owner roles
            user.ResourceOwnerRoles = _context.ResourceOwnerRoles
                .Include(r => r.Role)
                .Where(r => r.ResourceOwnerId == user.Id)
                .ToList();
            return user.ToDomain();
        }

        public bool Update(Domains.ResourceOwner resourceOwner)
        {
            var record = _context.ResourceOwners
                .Include(r => r.ResourceOwnerRoles)
                .FirstOrDefault(r => r.Id == resourceOwner.Id);
            if (record == null)
            {
                return false;
            }

            record.Name = resourceOwner.Name;
            record.Password = resourceOwner.Password;
            var rolesNotToBeDeleted = new List<string>();
            if (resourceOwner.Roles != null)
            {
                foreach(var role in resourceOwner.Roles)
                {
                    var regRole = record.ResourceOwnerRoles.FirstOrDefault(r => r.RoleName == role);
                    if (regRole == null)
                    {
                        regRole = new ResourceOwnerRole
                        {
                            RoleName = role,
                            ResourceOwnerId = resourceOwner.Id
                        };
                        record.ResourceOwnerRoles.Add(regRole);
                    }

                    rolesNotToBeDeleted.Add(regRole.RoleName);
                }
            }

            var roleNames = record.ResourceOwnerRoles.Select(r => r.RoleName).ToList();
            foreach(var roleName in roleNames.Where(r => !rolesNotToBeDeleted.Contains(r)))
            {
                record.ResourceOwnerRoles.Remove(record.ResourceOwnerRoles.First(r => r.RoleName == roleName));
            }

            _context.SaveChanges();
            return true;
        }

        public List<Domains.ResourceOwner> GetAll()
        {
            var users = _context.ResourceOwners.ToList();
            return users.Select(u => u.ToDomain()).ToList();
        }

        #endregion
    }
}
