using System.Collections.Generic;
using SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.Api.Configuration
{
    public static class Scopes
    {
        public static List<Scope> Get()
        {
            return new List<Scope>
            {
                new Scope
                {
                    Name = "BlogApi",
                    Description = "Access to the blog API",
                    IsInternal = false,
                    IsDisplayedInConsent = true
                },
                new Scope
                {
                    Name = "BlogApi:AddArticle",
                    Description = "Access to the add article operation",
                    IsInternal = false,
                    IsDisplayedInConsent = true
                },
                new Scope
                {
                    Name = "openid",
                    IsInternal = true,
                    IsDisplayedInConsent = false
                },
                new Scope
                {
                    Name = "profile",
                    IsExposed = true,
                    IsInternal = true,
                    Description = "Access to the profile",
                    Claims = new List<string>
                    {
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.WebSite,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt
                    },
                    Type = ScopeType.ResourceOwner,
                    IsDisplayedInConsent = true
                }
            };
        } 
    }
}