using Moq;
using SimpleIdentityServer.Core.Api.Discovery.Actions;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Discovery
{
    public class CreateDiscoveryDocumentationActionFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;
        private Mock<IClaimRepository> _claimRepositoryStub;
        private ICreateDiscoveryDocumentationAction _createDiscoveryDocumentationAction;

        [Fact]
        public async Task When_Expose_Two_Scopes_Then_DiscoveryDocument_Is_Correct()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string firstScopeName = "firstScopeName";
            const string secondScopeName = "secondScopeName";
            const string notExposedScopeName = "notExposedScopeName";
            ICollection<Scope> scopes = new List<Scope>
            {
                new Scope
                {
                    IsExposed = true,
                    Name = firstScopeName
                },
                new Scope
                {
                    IsExposed = true,
                    Name = secondScopeName
                },
                new Scope
                {
                    IsExposed = false,
                    Name = secondScopeName
                }
            };
            IEnumerable<ClaimAggregate> claims = new List<ClaimAggregate>
            {
                new ClaimAggregate
                {
                    Code = "claim"
                }
            };
            _scopeRepositoryStub.Setup(s => s.GetAllAsync())
                .Returns(Task.FromResult(scopes));
            _claimRepositoryStub.Setup(c => c.GetAllAsync())
                .Returns(() => Task.FromResult(claims));

            // ACT
            var discoveryInformation = await _createDiscoveryDocumentationAction.Execute();

            // ASSERT
            Assert.NotNull(discoveryInformation);
            Assert.True(discoveryInformation.ScopesSupported.Count() == 2);
            Assert.True(discoveryInformation.ScopesSupported.Contains(firstScopeName));
            Assert.True(discoveryInformation.ScopesSupported.Contains(secondScopeName));
            Assert.False(discoveryInformation.ScopesSupported.Contains(notExposedScopeName));
        }

        private void InitializeFakeObjects()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _claimRepositoryStub = new Mock<IClaimRepository>();
            _createDiscoveryDocumentationAction = new CreateDiscoveryDocumentationAction(
                _scopeRepositoryStub.Object,
                _claimRepositoryStub.Object);
        }
    }
}
