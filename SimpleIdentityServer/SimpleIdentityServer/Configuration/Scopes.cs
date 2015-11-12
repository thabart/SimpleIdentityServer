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
                        Core.Constants.StandardResourceOwnerClaimNames.Name,
                        Core.Constants.StandardResourceOwnerClaimNames.FamilyName,
                        Core.Constants.StandardResourceOwnerClaimNames.GivenName,
                        Core.Constants.StandardResourceOwnerClaimNames.MiddleName,
                        Core.Constants.StandardResourceOwnerClaimNames.NickName,
                        Core.Constants.StandardResourceOwnerClaimNames.PreferredUserName,
                        Core.Constants.StandardResourceOwnerClaimNames.Profile,
                        Core.Constants.StandardResourceOwnerClaimNames.Picture,
                        Core.Constants.StandardResourceOwnerClaimNames.WebSite,
                        Core.Constants.StandardResourceOwnerClaimNames.Gender,
                        Core.Constants.StandardResourceOwnerClaimNames.BirthDate,
                        Core.Constants.StandardResourceOwnerClaimNames.ZoneInfo,
                        Core.Constants.StandardResourceOwnerClaimNames.Locale,
                        Core.Constants.StandardResourceOwnerClaimNames.UpdatedAt
                    },
                    Type = ScopeType.ResourceOwner,
                    IsDisplayedInConsent = true
                }
            };
        } 
    }
}