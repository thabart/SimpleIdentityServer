using Moq;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.WebSite.User.Actions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.User
{
    public class UpdateUserTwoFactorAuthenticatorOperationFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IAuthenticateResourceOwnerService> _authenticateResourceOwnerServiceStub;
        private IUpdateUserTwoFactorAuthenticatorOperation _updateUserTwoFactorAuthenticatorOperation;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateUserTwoFactorAuthenticatorOperation.Execute(null, null));
        }

        [Fact]
        public async Task When_ResourceOwner_DoesntExist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwnerAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((ResourceOwner)null));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _updateUserTwoFactorAuthenticatorOperation.Execute("subject", "two_factor"));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.True(exception.Code == Errors.ErrorCodes.InternalError);
            Assert.True(exception.Message == Errors.ErrorDescriptions.TheRoDoesntExist);
        }

        [Fact]
        public async Task When_Passing_Correct_Parameters_Then_ResourceOwnerIs_Updated()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwnerAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner()));

            // ACT
            await _updateUserTwoFactorAuthenticatorOperation.Execute("subject", "two_factor");

            // ASSERTS
            _resourceOwnerRepositoryStub.Setup(r => r.UpdateAsync(It.IsAny<ResourceOwner>()));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _authenticateResourceOwnerServiceStub = new Mock<IAuthenticateResourceOwnerService>();
            _updateUserTwoFactorAuthenticatorOperation = new UpdateUserTwoFactorAuthenticatorOperation(
                _resourceOwnerRepositoryStub.Object,
                _authenticateResourceOwnerServiceStub.Object);
        }
    }
}
