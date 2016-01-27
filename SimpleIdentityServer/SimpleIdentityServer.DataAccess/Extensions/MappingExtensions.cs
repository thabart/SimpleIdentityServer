using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt;

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

        public static Domain.GrantedToken ToDomain(this Model.GrantedToken grantedToken)
        {
            return new Domain.GrantedToken
            {
                AccessToken = grantedToken.AccessToken,
                ClientId = grantedToken.ClientId,
                CreateDateTime = grantedToken.CreateDateTime,
                ExpiresIn = grantedToken.ExpiresIn,
                RefreshToken = grantedToken.RefreshToken,
                Scope = grantedToken.Scope,
                IdTokenPayLoad = string.IsNullOrWhiteSpace(grantedToken.IdTokenPayLoad) ? null : grantedToken.IdTokenPayLoad.DeserializeWithJavascript<JwsPayload>(),
                UserInfoPayLoad = string.IsNullOrWhiteSpace(grantedToken.UserInfoPayLoad) ? null : grantedToken.UserInfoPayLoad.DeserializeWithJavascript<JwsPayload>()
            };
        }

        public static Domain.Client ToDomain(this Model.Client client)
        {
            var scopes = new List<Domain.Scope>();
            var jsonWebKeys = new List<Jwt.JsonWebKey>();
            var grantTypes = new List<Domain.GrantType>();
            var responseTypes = new List<Domain.ResponseType>();

            if (client.AllowedScopes != null)
            {
                client.AllowedScopes.ToList().ForEach(scope => scopes.Add(scope.ToDomain()));
            }

            if (client.JsonWebKeys != null)
            {
                client.JsonWebKeys.ToList().ForEach(jsonWebKey => jsonWebKeys.Add(jsonWebKey.ToDomain()));
            }

            GetList(client.GrantTypes).ForEach(grantType =>
            {
                Domain.GrantType grantTypeEnum;
                if (Enum.TryParse(grantType, out grantTypeEnum))
                {
                    grantTypes.Add(grantTypeEnum);
                }
            });

            GetList(client.ResponseTypes).ForEach(responseType =>
            {
                Domain.ResponseType responseTypeEnum;
                if (Enum.TryParse(responseType, out responseTypeEnum))
                {
                    responseTypes.Add(responseTypeEnum);
                }
            });

            return new Domain.Client
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                ClientUri = client.ClientUri,
                ClientSecret = client.ClientSecret,
                IdTokenEncryptedResponseAlg = client.IdTokenEncryptedResponseAlg,
                IdTokenEncryptedResponseEnc = client.IdTokenEncryptedResponseEnc,
                JwksUri = client.JwksUri,
                TosUri = client.TosUri,
                LogoUri = client.LogoUri,
                PolicyUri = client.PolicyUri,
                RequestObjectEncryptionAlg = client.RequestObjectEncryptionAlg,
                RequestObjectEncryptionEnc = client.RequestObjectEncryptionEnc,
                IdTokenSignedResponseAlg = client.IdTokenSignedResponseAlg,
                RequireAuthTime = client.RequireAuthTime,
                SectorIdentifierUri = client.SectorIdentifierUri,
                SubjectType = client.SubjectType,
                TokenEndPointAuthSigningAlg = client.TokenEndPointAuthSigningAlg,
                UserInfoEncryptedResponseAlg = client.UserInfoEncryptedResponseAlg,
                UserInfoSignedResponseAlg = client.UserInfoSignedResponseAlg,
                UserInfoEncryptedResponseEnc = client.UserInfoEncryptedResponseEnc,
                DefaultMaxAge = client.DefaultMaxAge,
                DefaultAcrValues = client.DefaultAcrValues,
                InitiateLoginUri = client.InitiateLoginUri,
                RequestObjectSigningAlg = client.RequestObjectSigningAlg,
                TokenEndPointAuthMethod = (Domain.TokenEndPointAuthenticationMethods)client.TokenEndPointAuthMethod,
                ApplicationType = (Domain.ApplicationTypes)client.ApplicationType,
                RequestUris = GetList(client.RequestUris),
                RedirectionUrls = GetList(client.RedirectionUrls),
                Contacts = GetList(client.Contacts),
                AllowedScopes = scopes,
                JsonWebKeys = jsonWebKeys,
                GrantTypes = grantTypes,
                ResponseTypes = responseTypes
            };
        }

        public static Domain.Consent ToDomain(this Model.Consent consent)
        {
            return new Domain.Consent
            {
                Id = consent.Id.ToString(CultureInfo.InvariantCulture),
                Client = consent.Client == null ? null : consent.Client.ToDomain(),
                Claims = consent.Claims == null ? null : consent.Claims.Select(c => c.Code).ToList(),
                ResourceOwner = consent.ResourceOwner == null ? null : consent.ResourceOwner.ToDomain(),
                GrantedScopes = consent.GrantedScopes == null ? null : consent.GrantedScopes.Select(s => s.ToDomain()).ToList()
            };
        }

        #endregion

        #region Private static methods

        private static List<string> GetList(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new List<string>();
            }

            return value.Split(',').ToList();
        } 

        #endregion
    }
}
