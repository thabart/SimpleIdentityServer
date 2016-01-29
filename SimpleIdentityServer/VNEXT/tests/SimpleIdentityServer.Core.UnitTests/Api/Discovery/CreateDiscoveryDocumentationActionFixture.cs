using System.Collections.Generic;
using System.Linq;
using Moq;
using SimpleIdentityServer.Core.Api.Discovery.Actions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Discovery
{
    public class CreateDiscoveryDocumentationActionFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryFake;

        private ICreateDiscoveryDocumentationAction _createDiscoveryDocumentationAction;

        [Fact]
        public void When_Expose_Two_Scopes_Then_DiscoveryDocument_Is_Correct()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string firstScopeName = "firstScopeName";
            const string secondScopeName = "secondScopeName";
            const string notExposedScopeName = "notExposedScopeName";
            var scopes = new List<Scope>
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
            _scopeRepositoryFake.Setup(s => s.GetAllScopes())
                .Returns(scopes);

            // ACT
            var discoveryInformation = _createDiscoveryDocumentationAction.Execute();

            // ASSERT
            Assert.NotNull(discoveryInformation);
            Assert.True(discoveryInformation.ScopesSupported.Count() == 2);
            Assert.True(discoveryInformation.ScopesSupported.Contains(firstScopeName));
            Assert.True(discoveryInformation.ScopesSupported.Contains(secondScopeName));
            Assert.False(discoveryInformation.ScopesSupported.Contains(notExposedScopeName));
        }

        private void InitializeFakeObjects()
        {
            _scopeRepositoryFake = new Mock<IScopeRepository>();
            _createDiscoveryDocumentationAction = new CreateDiscoveryDocumentationAction(_scopeRepositoryFake.Object);
        }
    }
}
