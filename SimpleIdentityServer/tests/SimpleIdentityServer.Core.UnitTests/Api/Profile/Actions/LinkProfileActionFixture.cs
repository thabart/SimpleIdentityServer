using Moq;
using SimpleIdentityServer.Core.Api.Profile.Actions;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Profile.Actions
{
    public class LinkProfileActionFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IProfileRepository> _profileRepositoryStub;
        private ILinkProfileAction _linkProfileAction;

        [Fact]
        public async Task WhenPassingNullParametersThenExceptionAreThrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _linkProfileAction.Execute(null, null, null, false));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _linkProfileAction.Execute("localSubject", null, null, false));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _linkProfileAction.Execute("localSubject", "externalSubject", null, false));
        }

        [Fact]
        public async Task WhenResourceOwnerDoesntExistThenExceptionIsThrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>())).Returns(Task.FromResult((ResourceOwner)null));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _linkProfileAction.Execute("localSubject", "externalSubject", "issuer", false));

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(Errors.ErrorCodes.InternalError, exception.Code);
            Assert.Equal(Errors.ErrorDescriptions.TheResourceOwnerDoesntExist, exception.Message);
        }

        [Fact]
        public async Task WhenLinkingExistingProfileThenExceptionIsThrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwner()));
            _profileRepositoryStub.Setup(p => p.Get(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwnerProfile
            {
                ResourceOwnerId = "otherSubject"
            }));

            // ACT
            var exception = await Assert.ThrowsAsync<ProfileAssignedAnotherAccountException>(() => _linkProfileAction.Execute("localSubject", "externalSubject", "issuer", false));

            // ASSERT
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task WhenProfileHasAlreadyBeenLinkedThenExceptionIsThrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwner()));
            _profileRepositoryStub.Setup(p => p.Get(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwnerProfile
            {
                ResourceOwnerId = "localSubject"
            }));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _linkProfileAction.Execute("localSubject", "externalSubject", "issuer", false));

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(Errors.ErrorCodes.InternalError, exception.Code);
            Assert.Equal(Errors.ErrorDescriptions.TheProfileAlreadyLinked, exception.Message);
        }

        [Fact]
        public async Task WhenLinkProfileThenOperationIsCalled()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwner()));
            _profileRepositoryStub.Setup(p => p.Get(It.IsAny<string>())).Returns(Task.FromResult((ResourceOwnerProfile)null));

            // ACT
            await _linkProfileAction.Execute("localSubject", "externalSubject", "issuer", false);

            // ASSERT
            _profileRepositoryStub.Verify(p => p.Add(It.Is<IEnumerable<ResourceOwnerProfile>>(r => r.First().ResourceOwnerId == "localSubject" && r.First().Subject == "externalSubject" && r.First().Issuer == "issuer")));
        }

        [Fact]
        public async Task WhenForceLinkProfileThenOperationIsCalled()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwner()));
            _profileRepositoryStub.Setup(p => p.Get(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwnerProfile
            {
                ResourceOwnerId = "otherSubject"
            }));

            // ACT
            await _linkProfileAction.Execute("localSubject", "externalSubject", "issuer", true);

            // ASSERT
            _profileRepositoryStub.Verify(p => p.Remove(It.Is<IEnumerable<string>>(r => r.Contains("externalSubject"))));
            _profileRepositoryStub.Verify(p => p.Add(It.Is<IEnumerable<ResourceOwnerProfile>>(r => r.First().ResourceOwnerId == "localSubject" && r.First().Subject == "externalSubject" && r.First().Issuer == "issuer")));

        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _profileRepositoryStub = new Mock<IProfileRepository>();
            _linkProfileAction = new LinkProfileAction(_resourceOwnerRepositoryStub.Object, _profileRepositoryStub.Object);
        }
    }
}