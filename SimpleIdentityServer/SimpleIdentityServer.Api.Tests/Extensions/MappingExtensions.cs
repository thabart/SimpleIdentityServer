using SimpleIdentityServer.Api.Tests.Common.Fakes.Models;
using SimpleIdentityServer.DataAccess.Fake.Models;
using System.Linq;

namespace SimpleIdentityServer.Api.Tests.Extensions
{
    public static class MappingExtensions
    {
        public static Scope ToFake(this FakeScope scope)
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
    }
}
