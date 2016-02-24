using System;
using System.Linq;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using Domains = SimpleIdentityServer.Core.Models;
using Microsoft.Data.Entity;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class ResourceOwnerRepository : IResourceOwnerRepository
    {
         private readonly SimpleIdentityServerContext _context;
        
        public ResourceOwnerRepository(SimpleIdentityServerContext context) {
            _context = context;
        }
        
        public Domains.ResourceOwner GetResourceOwnerByCredentials(
            string userName, 
            string hashedPassword)
        {
                var user = _context.ResourceOwners.FirstOrDefault(r => r.Name == userName && r.Password == hashedPassword);
                if (user == null)
                {
                    return null;
                }

                return user.ToDomain();
        }

        public bool Insert(Domains.ResourceOwner resourceOwner)
        {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
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
                            Id = resourceOwner.Id
                        };

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

                        _context.ResourceOwners.Add(user);
                        _context.SaveChangesAsync();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

            return true;
        }

        public Domains.ResourceOwner GetBySubject(string subject)
        {
                var user = _context.ResourceOwners.FirstOrDefault(r => r.Id == subject);
                if (user == null)
                {
                    return null;
                }

                return user.ToDomain();
        }
    }
}
