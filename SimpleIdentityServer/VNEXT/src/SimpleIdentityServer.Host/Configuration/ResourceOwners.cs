using System;
using System.Collections.Generic;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Host.Configuration
{
    public static class ResourceOwners
    {
        public static List<ResourceOwner> Get()
        {
            return new List<ResourceOwner>
            {
                new ResourceOwner
                {
                    Id = "administrator@hotmail.be",
                    Name = "administrator",
                    Address = new Address
                    {
                        Country  = "France"
                    },
                    BirthDate = "1989-10-07",
                    Email = "habarthierry@hotmail.fr",
                    EmailVerified = true,
                    FamilyName = "habart",
                    Gender = "M",
                    GivenName = "Habart Thierry",
                    Locale = "fr-FR",
                    MiddleName = "Thierry",
                    NickName = "Titi",
                    PhoneNumber = "00",
                    PhoneNumberVerified = false,
                    Picture = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/58/Shiba_inu_taiki.jpg/220px-Shiba_inu_taiki.jpg",
                    PreferredUserName = "Thierry",
                    Profile = "http://localhost/profile",
                    UpdatedAt = DateTime.Now.ConvertToUnixTimestamp(),
                    WebSite = "https://github.com/thabart",
                    ZoneInfo = "Europe/Paris",
                    Password = "5E884898DA28047151D0E56F8DC6292773603D0D6AABBDD62A11EF721D1542D8"
                }
            };
        } 
    }
}