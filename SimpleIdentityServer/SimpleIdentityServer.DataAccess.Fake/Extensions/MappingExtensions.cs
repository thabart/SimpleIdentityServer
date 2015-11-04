using System.Linq;

using FAKEMODELS = SimpleIdentityServer.DataAccess.Fake.Models;
using MODELS = SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.DataAccess.Fake.Extensions
{
    public static class MappingExtensions
    {
        #region Fake mappings

        public static FAKEMODELS.Client ToFake(this MODELS.Client client)
        {
            return new FAKEMODELS.Client
            {
                ClientId = client.ClientId,
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
                Value = authorizationCode.Value,
                CreateDateTime = authorizationCode.CreateDateTime,
                Consent = authorizationCode.Consent.ToFake()
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
                IsInternal = scope.IsInternal
            };
        }

        #endregion

        #region Business mappings
        
        public static MODELS.Client ToBusiness(this FAKEMODELS.Client client)
        {
            return new MODELS.Client
            {
                ClientId = client.ClientId,
                DisplayName = client.DisplayName,
                AllowedScopes = client.AllowedScopes == null ? null : client.AllowedScopes.Select(s => s.ToBusiness()).ToList(),
                RedirectionUrls = client.RedirectionUrls == null ? null : client.RedirectionUrls.Select(r => r.Url).ToList(),
                ClientUri = client.ClientUri,
                LogoUri = client.LogoUri,
                TosUri = client.TosUri,
                PolicyUri = client.PolicyUri,
                GrantTypes = client.GrantTypes == null ? null : client.GrantTypes.Select(gt => gt.ToBusiness()).ToList(),
                ResponseTypes = client.ResponseTypes == null ? null : client.ResponseTypes.Select(rt => rt.ToBusiness()).ToList()
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
                Value = authorizationCode.Value,
                CreateDateTime = authorizationCode.CreateDateTime,
                Consent = authorizationCode.Consent.ToBusiness()
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
                IsInternal = scope.IsInternal
            };
        }

        public static MODELS.GrantType ToBusiness(this FAKEMODELS.GrantType grantType)
        {
            return (MODELS.GrantType)grantType;
        }

        public static MODELS.ResponseType ToBusiness(this FAKEMODELS.ResponseType responseType)
        {
            return (MODELS.ResponseType)responseType;
        }

        #endregion
    }
}
