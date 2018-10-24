using SimpleIdentityServer.Core.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SimpleIdentityServer.Core.Extensions
{
    internal static class CloneExtensions
    {
        public static ResourceOwnerProfile Copy(this ResourceOwnerProfile profile)
        {
            return new ResourceOwnerProfile
            {
                CreateDateTime = profile.CreateDateTime,
                Issuer = profile.Issuer,
                ResourceOwnerId = profile.ResourceOwnerId,
                Subject = profile.Subject,
                UpdateTime = profile.UpdateTime
            };
        }

        public static ClaimAggregate Copy(this ClaimAggregate claim)
        {
            return new ClaimAggregate
            {
                Code = claim.Code,
                CreateDateTime = claim.CreateDateTime,
                IsIdentifier = claim.IsIdentifier,
                UpdateDateTime = claim.UpdateDateTime,
                Value = claim.Value
            };
        }

        public static Consent Copy(this Consent consent)
        {
            return new Consent
            {
                Claims =  consent.Claims == null ? new List<string>() : consent.Claims.ToList(),
                Client = consent.Client == null ? null : consent.Client.Copy(),
                GrantedScopes = consent.GrantedScopes == null ? null : consent.GrantedScopes.Select(s => s.Copy()).ToList(),
                Id =  consent.Id,
                ResourceOwner = consent.ResourceOwner == null ? null : consent.ResourceOwner.Copy()
            };
        }

        public static Common.Models.Translation Copy(this Common.Models.Translation translation)
        {
            return new Common.Models.Translation
            {
                Code = translation.Code,
                LanguageTag = translation.LanguageTag,
                Value = translation.Value
            };
        }

        public static ResourceOwner Copy(this ResourceOwner user)
        {
            return new ResourceOwner
            {
                Claims = user.Claims == null ? new List<Claim>() : user.Claims.Select(c => c.Copy()).ToList(),
                CreateDateTime = user.CreateDateTime,
                Id = user.Id,
                IsLocalAccount = user.IsLocalAccount,
                Password = user.Password,
                TwoFactorAuthentication = user.TwoFactorAuthentication,
                UpdateDateTime = user.UpdateDateTime
            };
        }

        public static Claim Copy(this Claim claim)
        {
            return new Claim(claim.Type, claim.Value);
        }

        public static Common.Models.Client Copy(this Common.Models.Client client)
        {
            return new Common.Models.Client
            {
                AllowedScopes = client.AllowedScopes == null ? new List<Scope>() : client.AllowedScopes.Select(s => s.Copy()).ToList(),
                ApplicationType = client.ApplicationType,
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                ClientUri = client.ClientUri,
                Contacts = client.Contacts == null ? new List<string>() : client.Contacts.ToList(),
                CreateDateTime = client.CreateDateTime,
                DefaultAcrValues = client.DefaultAcrValues,
                DefaultMaxAge = client.DefaultMaxAge,
                GrantTypes = client.GrantTypes == null ? new List<GrantType>() : client.GrantTypes.ToList(),
                IdTokenEncryptedResponseAlg = client.IdTokenEncryptedResponseAlg,
                IdTokenEncryptedResponseEnc = client.IdTokenEncryptedResponseEnc,
                IdTokenSignedResponseAlg = client.IdTokenSignedResponseAlg,
                InitiateLoginUri = client.InitiateLoginUri,
                JsonWebKeys = client.JsonWebKeys == null ? new List<Common.JsonWebKey>() : client.JsonWebKeys.Select(j => j.Copy()).ToList(),
                JwksUri = client.JwksUri,
                LogoUri = client.LogoUri,
                PolicyUri = client.PolicyUri,
                PostLogoutRedirectUris = client.PostLogoutRedirectUris == null ? new List<string>() : client.PostLogoutRedirectUris.ToList(),
                RedirectionUrls = client.RedirectionUrls == null ? new List<string>() : client.RedirectionUrls.ToList(),
                RequestObjectEncryptionAlg = client.RequestObjectEncryptionAlg,
                RequestObjectEncryptionEnc = client.RequestObjectEncryptionEnc,
                RequestObjectSigningAlg = client.RequestObjectSigningAlg,
                RequestUris = client.RequestUris == null  ? new List<string>() : client.RequestUris.ToList(),
                RequireAuthTime = client.RequireAuthTime,
                RequirePkce = client.RequirePkce,
                ResponseTypes = client.ResponseTypes == null ? new List<ResponseType>() : client.ResponseTypes.ToList(),
                ScimProfile = client.ScimProfile,
                Secrets = client.Secrets == null ? new List<ClientSecret>() : client.Secrets.Select(s => s.Copy()).ToList(),
                SectorIdentifierUri = client.SectorIdentifierUri,
                SubjectType = client.SubjectType,
                TokenEndPointAuthMethod = client.TokenEndPointAuthMethod,
                TokenEndPointAuthSigningAlg = client.TokenEndPointAuthSigningAlg,
                TosUri = client.TosUri,
                UpdateDateTime = client.UpdateDateTime,
                UserInfoEncryptedResponseAlg = client.UserInfoEncryptedResponseAlg,
                UserInfoEncryptedResponseEnc = client.UserInfoEncryptedResponseEnc,
                UserInfoSignedResponseAlg = client.UserInfoSignedResponseAlg
            };
        }

        public static ClientSecret Copy(this ClientSecret clientSecret)
        {
            return new ClientSecret
            {
                Type = clientSecret.Type,
                Value = clientSecret.Value
            };
        }

        public static Scope Copy(this Scope scope)
        {
            return new Scope
            {
                CreateDateTime = scope.CreateDateTime,
                Description = scope.Description,
                IsDisplayedInConsent = scope.IsDisplayedInConsent,
                IsExposed = scope.IsExposed,
                IsOpenIdScope = scope.IsOpenIdScope,
                Name = scope.Name,
                Type = scope.Type,
                UpdateDateTime = scope.UpdateDateTime,
                Claims = scope.Claims == null ? new List<string>() : scope.Claims.ToList()
            };
        }

        public static Common.JsonWebKey Copy(this Common.JsonWebKey jsonWebKey)
        {
            return new Common.JsonWebKey
            {
                Alg = jsonWebKey.Alg,
                KeyOps = jsonWebKey.KeyOps == null ? new Common.KeyOperations[0] : jsonWebKey.KeyOps.ToList().ToArray(),
                Kid = jsonWebKey.Kid,
                Kty = jsonWebKey.Kty,
                SerializedKey = jsonWebKey.SerializedKey,
                Use = jsonWebKey.Use,
                X5t = jsonWebKey.X5t,
                X5tS256 = jsonWebKey.X5tS256,
                X5u = jsonWebKey.X5u
            };
        }
    }
}
