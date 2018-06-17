using Moq;
using SimpleIdentityServer.Core.Api.Profile.Actions;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Profile.Actions
{
    public class GetResourceOwnerClaimsActionFixture
    {
        private Mock<IProfileRepository> _profileRepositoryStub;
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private IGetResourceOwnerClaimsAction _getResourceOwnerClaimsAction;

        [Fact]
        public async Task WhenPassNullParameterThenExceptionIsThrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getResourceOwnerClaimsAction.Execute(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getResourceOwnerClaimsAction.Execute(string.Empty));
        }

        [Fact]
        public async Task WhenProfileDoesntExistThenNullIsReturned()
        {
            // INITIALIZE
            InitializeFakeObjects();
            _profileRepositoryStub.Setup(p => p.Get(It.IsAny<string>())).Returns(Task.FromResult((ResourceOwnerProfile)null));

            // ACT
            var result = await _getResourceOwnerClaimsAction.Execute("externalSubject");

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task WhenProfileExistsThenResourceOwnerIsReturned()
        {
            // INITIALIZE
            InitializeFakeObjects();
            _profileRepositoryStub.Setup(p => p.Get(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwnerProfile()));
            _resourceOwnerRepositoryStub.Setup(p => p.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwner
            {
                Id = "id"
            }));

            // ACT
            var result = await _getResourceOwnerClaimsAction.Execute("externalSubject");

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("id", result.Id);
        }

        private void InitializeFakeObjects()
        {
            _profileRepositoryStub = new Mock<IProfileRepository>();
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _getResourceOwnerClaimsAction = new GetResourceOwnerClaimsAction(_profileRepositoryStub.Object,
                _resourceOwnerRepositoryStub.Object);
        }
    }
}
