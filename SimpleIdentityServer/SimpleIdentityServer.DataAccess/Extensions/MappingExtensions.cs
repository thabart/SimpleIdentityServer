using System;
using System.Collections.Generic;
using System.Linq;
using Domain = SimpleIdentityServer.Core.Models;
using Model = SimpleIdentityServer.DataAccess.SqlServer.Models;
using Jwt = SimpleIdentityServer.Core.Jwt;

namespace SimpleIdentityServer.DataAccess.SqlServer.Extensions
{
    public static class MappingExtensions
    {
        #region To Domain Objects

        public static Domain.Translation ToDomain(this Model.Translation translation)
        {
            return new Domain.Translation
            {
                Code = translation.Code,
                LanguageTag = translation.LanguageTag,
                Value = translation.Value
            };
        }

        public static Domain.Scope ToDomain(this Model.Scope scope)
        {
            return new Domain.Scope
            {
                Name = scope.Name,
                Description = scope.Description,
                IsDisplayedInConsent = scope.IsDisplayedInConsent,
                IsExposed = scope.IsExposed,
                IsOpenIdScope = scope.IsOpenIdScope,
                Type = (Domain.ScopeType)scope.Type,
                Claims = scope.Claims == null ? new List<string>() : scope.Claims.Select(c => c.Code).ToList()
            };
        }

        public static Domain.ResourceOwner ToDomain(this Model.ResourceOwner resourceOwner)
        {
            return new Domain.ResourceOwner
            {
                BirthDate = resourceOwner.BirthDate,
                Name = resourceOwner.Name,
                Email = resourceOwner.Email,
                EmailVerified = resourceOwner.EmailVerified,
                FamilyName = resourceOwner.FamilyName,
                Gender = resourceOwner.Gender,
                GivenName = resourceOwner.GivenName,
                Id = resourceOwner.Id,
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
                Address = resourceOwner.Address == null ? null : resourceOwner.Address.ToDomain()
            };
        }

        public static Domain.Address ToDomain(this Model.Address address)
        {
            return new Domain.Address
            {
                Country = address.Country,
                Formatted = address.Formatted,
                Locality = address.Locality,
                PostalCode = address.PostalCode,
                Region = address.Region
            };
        }

        public static Jwt.JsonWebKey ToDomain(this Model.JsonWebKey jsonWebKey)
        {
            Uri x5u = null;
            if (Uri.IsWellFormedUriString(jsonWebKey.X5u, UriKind.Absolute))
            {
                x5u = new Uri(jsonWebKey.X5u);
            }

            var keyOperationsEnums = new List<Jwt.KeyOperations>();
            if (!string.IsNullOrWhiteSpace(jsonWebKey.KeyOps))
            {
                var keyOperations = jsonWebKey.KeyOps.Split(',');
                foreach (var keyOperation in keyOperations)
                {
                    Jwt.KeyOperations keyOperationEnum;
                    if (!Enum.TryParse(keyOperation, out keyOperationEnum))
                    {
                        continue;
                    }

                    keyOperationsEnums.Add(keyOperationEnum);
                }
            }

            return new Jwt.JsonWebKey
            {
                Kid = jsonWebKey.Kid,
                Alg = (Jwt.AllAlg)jsonWebKey.Alg,
                Kty = (Jwt.KeyType)jsonWebKey.Kty,
                Use = (Jwt.Use)jsonWebKey.Use,
                X5t = jsonWebKey.X5t,
                X5tS256 = jsonWebKey.X5tS256,
                X5u = x5u,
                SerializedKey = jsonWebKey.SerializedKey,
                KeyOps = keyOperationsEnums.ToArray()
            };
        }

        #endregion
    }
}
