using System;
using System.Linq;
using SimpleIdentityServer.Core;
using FAKEMODELS = SimpleIdentityServer.DataAccess.Fake.Models;
using MODELS = SimpleIdentityServer.Core.Models;
using JSON = SimpleIdentityServer.Core.Jwt.Signature;

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
                IdTokenSignedTResponseAlg = client.IdTokenSignedTResponseAlg,
                ClientId = client.ClientId,
                ClientSecret = client.ClientSecret,
                TokenEndPointAuthMethod = client.TokenEndPointAuthMethod.ToFake(),
                DisplayName = client.DisplayName,
                AllowedScopes = client.AllowedScopes == null ? null : client.AllowedScopes.Select(s => s.ToFake()).ToList(),
                RedirectionUrls = client.RedirectionUrls == null ? null : client.RedirectionUrls.Select(r => new FAKEMODELS.RedirectionUrl
                {
                    Url = r
                }).ToList()
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
                TokenType = grantedToken.TokenType
            };
        }

        public static FAKEMODELS.ResourceOwner ToFake (this MODELS.ResourceOwner resourceOwner)
        {
            return new FAKEMODELS.ResourceOwner
            {
                Id = resourceOwner.Id,
                Password = resourceOwner.Password,
                UserName = resourceOwner.UserName
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
                IdToken = authorizationCode.IdToken,
                Scopes = authorizationCode.Scopes
            };
        }

        public static FAKEMODELS.Consent ToFake (this MODELS.Consent consent)
        {
            return new FAKEMODELS.Consent
            {
                Id = consent.Id,
                Client = consent.Client.ToFake(),
                ResourceOwner = consent.ResourceOwner.ToFake(),
                GrantedScopes = consent.GrantedScopes == null ? null : consent.GrantedScopes.Select(s => s.ToFake()).ToList()                
            };
        }

        public static FAKEMODELS.Scope ToFake(this MODELS.Scope scope)
        {
            return new FAKEMODELS.Scope
            {
                Name = scope.Name,
                Description = scope.Description,
                IsInternal = scope.IsInternal,
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
                DisplayName = client.DisplayName,
                AllowedScopes = client.AllowedScopes == null ? null : client.AllowedScopes.Select(s => s.ToBusiness()).ToList(),
                RedirectionUrls = client.RedirectionUrls == null ? null : client.RedirectionUrls.Select(r => r.Url).ToList(),
                ClientUri = client.ClientUri,
                LogoUri = client.LogoUri,
                TosUri = client.TosUri,
                PolicyUri = client.PolicyUri,
                GrantTypes = client.GrantTypes == null ? null : client.GrantTypes.Select(gt => gt.ToBusiness()).ToList(),
                ResponseTypes = client.ResponseTypes == null ? null : client.ResponseTypes.Select(rt => rt.ToBusiness()).ToList(),
                IdTokenEncryptedResponseAlg = client.IdTokenEncryptedResponseAlg,
                IdTokenEncryptedResponseEnc = client.IdTokenEncryptedResponseEnc,
                IdTokenSignedTResponseAlg = client.IdTokenSignedTResponseAlg,
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
                Password = resourceOwner.Password,
                UserName = resourceOwner.UserName
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
                IdToken = authorizationCode.IdToken,
                Scopes = authorizationCode.Scopes
            };
        }

        public static MODELS.Consent ToBusiness(this FAKEMODELS.Consent consent)
        {
            return new MODELS.Consent
            {
                Id = consent.Id,
                Client = consent.Client.ToBusiness(),
                ResourceOwner = consent.ResourceOwner.ToBusiness(),
                GrantedScopes = consent.GrantedScopes == null ? null : consent.GrantedScopes.Select(s => s.ToBusiness()).ToList()
            };
        }

        public static MODELS.Scope ToBusiness(this FAKEMODELS.Scope scope)
        {
            return new MODELS.Scope
            {
                Name = scope.Name,
                Description = scope.Description,
                IsInternal = scope.IsInternal,
                IsExposed = scope.IsExposed,
                Claims = scope.Claims,
                Type = scope.Type.ToBusiness(),
                IsDisplayedInConsent = scope.IsDisplayedInConsent
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
                TokenType = grantedToken.TokenType
            };
        }

        public static MODELS.ScopeType ToBusiness(this FAKEMODELS.ScopeType scopeType)
        {
            return (MODELS.ScopeType) scopeType;
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
