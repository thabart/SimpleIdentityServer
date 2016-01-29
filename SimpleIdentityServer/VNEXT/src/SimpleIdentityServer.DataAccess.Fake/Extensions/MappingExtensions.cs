using System;
using System.Linq;

using FAKEMODELS = SimpleIdentityServer.DataAccess.Fake.Models;
using MODELS = SimpleIdentityServer.Core.Models;
using JSON = SimpleIdentityServer.Core.Jwt;

namespace SimpleIdentityServer.DataAccess.Fake.Extensions
{
    public static class MappingExtensions
    {
        #region Fake mappings

        public static FAKEMODELS.Client ToFake(this MODELS.Client client)
        {
            return new FAKEMODELS.Client
            {
                IdTokenEncryptedResponseAlg = client.IdTokenEncryptedResponseAlg,
                IdTokenEncryptedResponseEnc = client.IdTokenEncryptedResponseEnc,
                IdTokenSignedResponseAlg = client.IdTokenSignedResponseAlg,
                ClientId = client.ClientId,
                ClientSecret = client.ClientSecret,
                TokenEndPointAuthMethod = client.TokenEndPointAuthMethod.ToFake(),
                ClientName = client.ClientName,
                AllowedScopes = client.AllowedScopes == null ? null : client.AllowedScopes.Select(s => s.ToFake()).ToList(),
                RedirectionUrls = client.RedirectionUrls,
                JwksUri = client.JwksUri,
                JsonWebKeys = client.JsonWebKeys == null ? null : client.JsonWebKeys.Select(s => s.ToFake()).ToList(),
                ApplicationType = client.ApplicationType.ToFake(),
                Contacts = client.Contacts,
                DefaultAcrValues = client.DefaultAcrValues,
                DefaultMaxAge = client.DefaultMaxAge,
                InitiateLoginUri = client.InitiateLoginUri,
                RequestObjectEncryptionAlg = client.RequestObjectEncryptionAlg,
                RequestObjectEncryptionEnc = client.RequestObjectEncryptionEnc,
                RequestObjectSigningAlg = client.RequestObjectSigningAlg,
                RequestUris = client.RequestUris,
                SectorIdentifierUri = client.SectorIdentifierUri,
                RequireAuthTime = client.RequireAuthTime,
                SubjectType = client.SubjectType,
                TokenEndPointAuthSigningAlg = client.TokenEndPointAuthSigningAlg,
                UserInfoEncryptedResponseAlg = client.UserInfoEncryptedResponseAlg,
                UserInfoEncryptedResponseEnc = client.UserInfoEncryptedResponseEnc,
                UserInfoSignedResponseAlg = client.UserInfoSignedResponseAlg,
                ClientUri = client.ClientUri,
                GrantTypes = client.GrantTypes == null ? null : client.GrantTypes.Select(g => g.ToFake()).ToList(),
                TosUri = client.TosUri,
                LogoUri = client.LogoUri,
                PolicyUri = client.PolicyUri,
                ResponseTypes = client.ResponseTypes == null ? null : client.ResponseTypes.Select(r => r.ToFake()).ToList()
            };
        }


        public static FAKEMODELS.JsonWebKey ToFake(this JSON.JsonWebKey jsonWebKey)
        {
            return new FAKEMODELS.JsonWebKey
            {
                X5u = jsonWebKey.X5u,
                X5tS256 = jsonWebKey.X5tS256,
                X5t = jsonWebKey.X5t,
                Kid = jsonWebKey.Kid,
                Alg = jsonWebKey.Alg.ToFake(),
                Kty = jsonWebKey.Kty.ToFake(),
                Use = jsonWebKey.Use.ToFake(),
                KeyOps = jsonWebKey.KeyOps == null ? null : jsonWebKey.KeyOps.Select(ko => ko.ToFake()).ToArray(),
                SerializedKey = jsonWebKey.SerializedKey
            };
        }

        public static FAKEMODELS.Translation ToFake(this MODELS.Translation translation)
        {
            return new FAKEMODELS.Translation
            {
                Code = translation.Code,
                LanguageTag = translation.LanguageTag,
                Value = translation.Value
            };
        }

        public static FAKEMODELS.RedirectionUrl ToFake(this MODELS.RedirectionUrl redirectionUrl)
        {
            return new FAKEMODELS.RedirectionUrl
            {
                Url = redirectionUrl.Url
            };
        }

        public static FAKEMODELS.GrantedToken ToFake(this MODELS.GrantedToken grantedToken)
        {
            return new FAKEMODELS.GrantedToken
            {
                AccessToken = grantedToken.AccessToken,
                ExpiresIn = grantedToken.ExpiresIn,
                IdToken = grantedToken.IdToken,
                RefreshToken = grantedToken.RefreshToken,
                Scope = grantedToken.Scope,
                TokenType = grantedToken.TokenType,
                CreateDateTime = grantedToken.CreateDateTime,
                UserInfoPayLoad = grantedToken.UserInfoPayLoad,
                IdTokenPayLoad = grantedToken.IdTokenPayLoad,
                ClientId = grantedToken.ClientId
            };
        }

        public static FAKEMODELS.ResourceOwner ToFake (this MODELS.ResourceOwner resourceOwner)
        {
            return new FAKEMODELS.ResourceOwner
            {
                Id = resourceOwner.Id,
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
                PhoneNumber = resourceOwner.PhoneNumber,
                PhoneNumberVerified = resourceOwner.PhoneNumberVerified,
                Picture = resourceOwner.Picture,
                PreferredUserName = resourceOwner.PreferredUserName,
                Profile = resourceOwner.Profile,
                UpdatedAt = resourceOwner.UpdatedAt,
                WebSite = resourceOwner.WebSite,
                ZoneInfo = resourceOwner.ZoneInfo,
                Password = resourceOwner.Password,
                Address = resourceOwner.Address == null ? null : resourceOwner.Address.ToFake()
            };
        }

        public static FAKEMODELS.Address ToFake(this MODELS.Address address)
        {
            return new FAKEMODELS.Address
            {
                Country = address.Country,
                Formatted = address.Formatted,
                Locality = address.Locality,
                PostalCode = address.PostalCode,
                Region = address.Region,
                StreetAddress = address.StreetAddress
            };
        }

        public static FAKEMODELS.AuthorizationCode ToFake (this MODELS.AuthorizationCode authorizationCode)
        {
            return new FAKEMODELS.AuthorizationCode
            {
                Code = authorizationCode.Code,
                RedirectUri = authorizationCode.RedirectUri,
                CreateDateTime = authorizationCode.CreateDateTime,
                ClientId = authorizationCode.ClientId,
                Scopes = authorizationCode.Scopes,
               UserInfoPayLoad = authorizationCode.UserInfoPayLoad,
               IdTokenPayload = authorizationCode.IdTokenPayload
            };
        }

        public static FAKEMODELS.Consent ToFake (this MODELS.Consent consent)
        {
            return new FAKEMODELS.Consent
            {
                Id = consent.Id,
                Client = consent.Client.ToFake(),
                ResourceOwner = consent.ResourceOwner.ToFake(),
                GrantedScopes = consent.GrantedScopes == null ? null : consent.GrantedScopes.Select(s => s.ToFake()).ToList(),
                Claims = consent.Claims              
            };
        }

        public static FAKEMODELS.Scope ToFake(this MODELS.Scope scope)
        {
            return new FAKEMODELS.Scope
            {
                Name = scope.Name,
                Description = scope.Description,
                IsOpenIdScope = scope.IsOpenIdScope,
                IsExposed = scope.IsExposed,
                Claims = scope.Claims,
                Type = scope.Type.ToFake(),
                IsDisplayedInConsent = scope.IsDisplayedInConsent
            };
        }

        public static FAKEMODELS.TokenEndPointAuthenticationMethods ToFake(this MODELS.TokenEndPointAuthenticationMethods tokenEdp)
        {
            return (FAKEMODELS.TokenEndPointAuthenticationMethods)tokenEdp;
        }

        public static FAKEMODELS.GrantType ToFake(this MODELS.GrantType grantType)
        {
            return (FAKEMODELS.GrantType)grantType;
        }

        public static FAKEMODELS.ResponseType ToFake(this MODELS.ResponseType responseType)
        {
            return (FAKEMODELS.ResponseType)responseType;
        }

        public static FAKEMODELS.ApplicationTypes ToFake(this MODELS.ApplicationTypes applicationType)
        {
            return (FAKEMODELS.ApplicationTypes)applicationType;
        }

        public static FAKEMODELS.ScopeType ToFake(this MODELS.ScopeType scopeType)
        {
            return (FAKEMODELS.ScopeType)scopeType;
        }

        public static FAKEMODELS.AllAlg ToFake(this JSON.AllAlg alg)
        {
            var algName = Enum.GetName(typeof(JSON.AllAlg), alg);
            return (FAKEMODELS.AllAlg)Enum.Parse(typeof(FAKEMODELS.AllAlg), algName);
        }

        public static FAKEMODELS.KeyType ToFake(this JSON.KeyType kt)
        {
            var ktName = Enum.GetName(typeof(JSON.KeyType), kt);
            return (FAKEMODELS.KeyType)Enum.Parse(typeof(FAKEMODELS.KeyType), ktName);
        }

        public static FAKEMODELS.Use ToFake(this JSON.Use use)
        {
            var useName = Enum.GetName(typeof(JSON.Use), use);
            return (FAKEMODELS.Use)Enum.Parse(typeof(FAKEMODELS.Use), useName);
        }

        public static FAKEMODELS.KeyOperations ToFake(this JSON.KeyOperations kop)
        {
            var kopName = Enum.GetName(typeof(JSON.KeyOperations), kop);
            return (FAKEMODELS.KeyOperations)Enum.Parse(typeof(FAKEMODELS.KeyOperations), kopName);
        }

        #endregion

        #region Business mappings
        
        public static MODELS.Client ToBusiness(this FAKEMODELS.Client client)
        {
            return new MODELS.Client
            {
                ClientId = client.ClientId,
                ClientSecret = client.ClientSecret,
                TokenEndPointAuthMethod = client.TokenEndPointAuthMethod.ToBusiness(),
                ClientName = client.ClientName,
                AllowedScopes = client.AllowedScopes == null ? null : client.AllowedScopes.Select(s => s.ToBusiness()).ToList(),
                RedirectionUrls = client.RedirectionUrls,
                ClientUri = client.ClientUri,
                LogoUri = client.LogoUri,
                TosUri = client.TosUri,
                PolicyUri = client.PolicyUri,
                GrantTypes = client.GrantTypes == null ? null : client.GrantTypes.Select(gt => gt.ToBusiness()).ToList(),
                ResponseTypes = client.ResponseTypes == null ? null : client.ResponseTypes.Select(rt => rt.ToBusiness()).ToList(),
                IdTokenEncryptedResponseAlg = client.IdTokenEncryptedResponseAlg,
                IdTokenEncryptedResponseEnc = client.IdTokenEncryptedResponseEnc,
                IdTokenSignedResponseAlg = client.IdTokenSignedResponseAlg,
                JwksUri = client.JwksUri,
                JsonWebKeys = client.JsonWebKeys == null ? null : client.JsonWebKeys.Select(j => j.ToBusiness()).ToList(),
                ApplicationType = client.ApplicationType.ToBusiness(),
                Contacts = client.Contacts,
                DefaultAcrValues = client.DefaultAcrValues,
                DefaultMaxAge = client.DefaultMaxAge,
                InitiateLoginUri = client.InitiateLoginUri,
                RequestObjectEncryptionAlg = client.RequestObjectEncryptionAlg,
                RequestObjectEncryptionEnc = client.RequestObjectEncryptionEnc,
                RequestObjectSigningAlg = client.RequestObjectSigningAlg,
                RequestUris = client.RequestUris,
                SectorIdentifierUri = client.SectorIdentifierUri,
                RequireAuthTime = client.RequireAuthTime,
                SubjectType = client.SubjectType,
                TokenEndPointAuthSigningAlg = client.TokenEndPointAuthSigningAlg,
                UserInfoEncryptedResponseAlg = client.UserInfoEncryptedResponseAlg,
                UserInfoEncryptedResponseEnc = client.UserInfoEncryptedResponseEnc,
                UserInfoSignedResponseAlg = client.UserInfoSignedResponseAlg
            };
        }

        public static MODELS.RedirectionUrl ToBusiness(this FAKEMODELS.RedirectionUrl redirectionUrl)
        {
            return new MODELS.RedirectionUrl
            {
                Url = redirectionUrl.Url
            };
        }

        public static MODELS.ResourceOwner ToBusiness(this FAKEMODELS.ResourceOwner resourceOwner)
        {
            return new MODELS.ResourceOwner
            {
                Id = resourceOwner.Id,
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
                PhoneNumber = resourceOwner.PhoneNumber,
                PhoneNumberVerified = resourceOwner.PhoneNumberVerified,
                Picture = resourceOwner.Picture,
                PreferredUserName = resourceOwner.PreferredUserName,
                Profile = resourceOwner.Profile,
                UpdatedAt = resourceOwner.UpdatedAt,
                WebSite = resourceOwner.WebSite,
                ZoneInfo = resourceOwner.ZoneInfo,
                Password = resourceOwner.Password,
                Address = resourceOwner.Address == null ? null : resourceOwner.Address.ToBusiness()
            };
        }

        public static MODELS.AuthorizationCode ToBusiness(this FAKEMODELS.AuthorizationCode authorizationCode)
        {
            return new MODELS.AuthorizationCode
            {
                Code = authorizationCode.Code,
                RedirectUri = authorizationCode.RedirectUri,
                CreateDateTime = authorizationCode.CreateDateTime,
                ClientId = authorizationCode.ClientId,
                Scopes = authorizationCode.Scopes,
                UserInfoPayLoad = authorizationCode.UserInfoPayLoad,
                IdTokenPayload = authorizationCode.IdTokenPayload
            };
        }

        public static MODELS.Consent ToBusiness(this FAKEMODELS.Consent consent)
        {
            return new MODELS.Consent
            {
                Id = consent.Id,
                Client = consent.Client.ToBusiness(),
                ResourceOwner = consent.ResourceOwner.ToBusiness(),
                GrantedScopes = consent.GrantedScopes == null ? null : consent.GrantedScopes.Select(s => s.ToBusiness()).ToList(),
                Claims = consent.Claims
            };
        }

        public static MODELS.Scope ToBusiness(this FAKEMODELS.Scope scope)
        {
            return new MODELS.Scope
            {
                Name = scope.Name,
                Description = scope.Description,
                IsOpenIdScope = scope.IsOpenIdScope,
                IsExposed = scope.IsExposed,
                Claims = scope.Claims,
                Type = scope.Type.ToBusiness(),
                IsDisplayedInConsent = scope.IsDisplayedInConsent
            };
        }
        
        public static MODELS.Translation ToBusiness(this FAKEMODELS.Translation translation)
        {
            if (translation == null)
            {
                return null;
            }

            return new MODELS.Translation
            {
                Code = translation.Code,
                LanguageTag = translation.LanguageTag,
                Value = translation.Value
            };
        }

        public static MODELS.GrantedToken ToBusiness(this FAKEMODELS.GrantedToken grantedToken)
        {
            return new MODELS.GrantedToken
            {
                AccessToken = grantedToken.AccessToken,
                ExpiresIn = grantedToken.ExpiresIn,
                IdToken = grantedToken.IdToken,
                RefreshToken = grantedToken.RefreshToken,
                Scope = grantedToken.Scope,
                TokenType = grantedToken.TokenType,
                CreateDateTime = grantedToken.CreateDateTime,
                UserInfoPayLoad = grantedToken.UserInfoPayLoad,
                ClientId = grantedToken.ClientId,
                IdTokenPayLoad = grantedToken.IdTokenPayLoad
            };
        }

        public static MODELS.Address ToBusiness(this FAKEMODELS.Address address)
        {
            return new MODELS.Address
            {
                Country = address.Country,
                Formatted = address.Formatted,
                Locality = address.Locality,
                PostalCode = address.PostalCode,
                Region = address.Region,
                StreetAddress = address.StreetAddress
            };
        }

        public static MODELS.ScopeType ToBusiness(this FAKEMODELS.ScopeType scopeType)
        {
            return (MODELS.ScopeType) scopeType;
        }

        public static MODELS.ApplicationTypes ToBusiness(this FAKEMODELS.ApplicationTypes applicationType)
        {
            return (MODELS.ApplicationTypes) applicationType;
        }

        public static MODELS.TokenEndPointAuthenticationMethods ToBusiness(this FAKEMODELS.TokenEndPointAuthenticationMethods tokenEdp)
        {
            return (MODELS.TokenEndPointAuthenticationMethods)tokenEdp;
        }

        public static MODELS.GrantType ToBusiness(this FAKEMODELS.GrantType grantType)
        {
            return (MODELS.GrantType)grantType;
        }

        public static MODELS.ResponseType ToBusiness(this FAKEMODELS.ResponseType responseType)
        {
            return (MODELS.ResponseType)responseType;
        }

        public static JSON.JsonWebKey ToBusiness(this FAKEMODELS.JsonWebKey jsonWebKey)
        {
            return new JSON.JsonWebKey
            {
                X5u = jsonWebKey.X5u,
                X5tS256 = jsonWebKey.X5tS256,
                X5t = jsonWebKey.X5t,
                Kid = jsonWebKey.Kid,
                Alg = jsonWebKey.Alg.ToBusiness(),
                Kty = jsonWebKey.Kty.ToBusiness(),
                Use = jsonWebKey.Use.ToBusiness(),
                KeyOps = jsonWebKey.KeyOps == null ? null : jsonWebKey.KeyOps.Select(ko => ko.ToBusiness()).ToArray(),
                SerializedKey = jsonWebKey.SerializedKey
            };
        }

        public static JSON.AllAlg ToBusiness(this FAKEMODELS.AllAlg alg)
        {
            var algName = Enum.GetName(typeof (FAKEMODELS.AllAlg), alg);
            return (JSON.AllAlg)Enum.Parse(typeof(JSON.AllAlg), algName);
        }

        public static JSON.KeyType ToBusiness(this FAKEMODELS.KeyType kt)
        {
            var ktName = Enum.GetName(typeof (FAKEMODELS.KeyType), kt);
            return (JSON.KeyType) Enum.Parse(typeof (JSON.KeyType), ktName);
        }

        public static JSON.Use ToBusiness(this FAKEMODELS.Use use)
        {
            var useName = Enum.GetName(typeof (FAKEMODELS.Use), use);
            return (JSON.Use) Enum.Parse(typeof (JSON.Use), useName);
        }

        public static JSON.KeyOperations ToBusiness(this FAKEMODELS.KeyOperations kop)
        {
            var kopName = Enum.GetName(typeof(FAKEMODELS.KeyOperations), kop);
            return (JSON.KeyOperations) Enum.Parse(typeof (JSON.KeyOperations), kopName);
        }

        #endregion
    }
}
