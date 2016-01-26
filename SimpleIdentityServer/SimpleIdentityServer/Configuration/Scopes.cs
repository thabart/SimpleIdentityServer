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
                    IsOpenIdScope = false,
                    IsDisplayedInConsent = true
                },
                new Scope
                {
                    Name = "BlogApi:AddArticle",
                    Description = "Access to the add article operation",
                    IsOpenIdScope = false,
                    IsDisplayedInConsent = true
                },
                new Scope
                {
                    Name = "openid",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = true,
                    Description = "access to the openid scope",
                    Type = ScopeType.ProtectedApi
                },
                new Scope
                {
                    Name = "profile",
                    IsExposed = true,
                    IsOpenIdScope = true,
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
                },
                new Scope
                {
                    Name = "email",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the email",
                    Claims = new List<string>
                    {
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified
                    },
                    Type = ScopeType.ResourceOwner
                },
                new Scope
                {
                    Name = "address",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the address",
                    Claims = new List<string>
                    {
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.Address
                    },
                    Type = ScopeType.ResourceOwner
                },
                new Scope
                {
                    Name = "phone",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the phone",
                    Claims = new List<string>
                    {
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber,
                        Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified
                    },
                    Type = ScopeType.ResourceOwner
                }
            };
        } 
    }
}