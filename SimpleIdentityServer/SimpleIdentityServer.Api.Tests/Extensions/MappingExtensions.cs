using SimpleIdentityServer.Api.Tests.Common.Fakes.Models;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.DataAccess.Fake.Models;
using System.Linq;

using FAKE = SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.Api.Tests.Extensions
{
    public static class MappingExtensions
    {
        public static FAKE.Scope ToFake(this FakeScope scope)
        {
            return new Scope
            {
                Description = scope.Description,
                IsExposed = scope.IsExposed,
                IsDisplayedInConsent = scope.IsDisplayedInConsent,
                Name = scope.Name,
                Type = (ScopeType)scope.Type,
                IsInternal = scope.IsInternal,
                Claims = string.IsNullOrWhiteSpace(scope.Claims) ? null : scope.Claims.Split(' ').ToList()
            };
        }

        public static JwsPayload ToBusiness(this FakeJwsPayload jwsPayload)
        {
            return new JwsPayload
            {
                {
                    Core.Jwt.Constants.StandardClaimNames.Issuer, jwsPayload.iss
                },
                {
                    Core.Jwt.Constants.StandardClaimNames.Jti, jwsPayload.jti
                },
                {
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, jwsPayload.sub
                }
            };
        }
    }
}
