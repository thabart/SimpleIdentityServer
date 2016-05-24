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