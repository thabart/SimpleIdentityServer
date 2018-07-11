using Moq;
using SimpleIdentityServer.Authenticate.SMS.Services;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Store;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Authenticate.SMS.Tests.Services
{
    public class SmsAuthenticateResourceOwnerServiceFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IConfirmationCodeStore> _confirmationCodeStoreStub;
        private IAuthenticateResourceOwnerService _authenticateResourceOwnerService;

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync("login", null));
        }

        [Fact]
        public async Task When_ConfirmationCode_Doesnt_Exist_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _confirmationCodeStoreStub.Setup(c => c.Get(It.IsAny<string>())).Returns(() => Task.FromResult((ConfirmationCode)null));

            // ACT
            var result = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync("login", "password");

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task When_Subject_Is_Different_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _confirmationCodeStoreStub.Setup(c => c.Get(It.IsAny<string>())).Returns(() => Task.FromResult(new ConfirmationCode
            {
                Subject = "sub"
            }));

            // ACT
            var result = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync("login", "password");

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task When_ConfirmationCode_Is_Expired_Then_Null_Is_Returned()
        {
            const string login = "login";
            // ARRANGE
            InitializeFakeObjects();
            _confirmationCodeStoreStub.Setup(c => c.Get(It.IsAny<string>())).Returns(() => Task.FromResult(new ConfirmationCode
            {
                Subject = login,
                IssueAt = DateTime.UtcNow.AddDays(-1),
                ExpiresIn = 100
            }));

            // ACT
            var result = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(login, "password");

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task When_ConfirmationCode_Is_Correct_And_PhoneNumber_Correct_Then_Operation_Is_Called()
        {
            const string login = "login";
            // ARRANGE
            InitializeFakeObjects();
            _confirmationCodeStoreStub.Setup(c => c.Get(It.IsAny<string>())).Returns(() => Task.FromResult(new ConfirmationCode
            {
                Subject = login,
                IssueAt = DateTime.UtcNow,
                ExpiresIn = 100
            }));

            // ACT
            await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(login, "password");

            // ASSERT
            _resourceOwnerRepositoryStub.Verify(r => r.GetResourceOwnerByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, login));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _confirmationCodeStoreStub = new Mock<IConfirmationCodeStore>();
            _authenticateResourceOwnerService = new SmsAuthenticateResourceOwnerService(_resourceOwnerRepositoryStub.Object,
                _confirmationCodeStoreStub.Object);
        }
    }
}
