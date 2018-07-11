using Moq;
using SimpleIdentityServer.Authenticate.SMS.Actions;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.WebSite.User;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Authenticate.SMS.Tests.Actions
{
    public class SmsAuthenticationOperationFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IUserActions> _userActionsStub;
        private SmsAuthenticationOptions _smsAuthenticationOptions;
        private ISmsAuthenticationOperation _smsAuthenticationOperation;

        [Fact]
        public async Task When_Null_Parameter_Is_Passed_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _smsAuthenticationOperation.Execute(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _smsAuthenticationOperation.Execute(string.Empty));
        }

        [Fact]
        public async Task When_ResourceOwner_Exists_Then_ResourceOwner_Is_Returned()
        {
            // ARRANGE
            const string phone = "phone";
            var resourceOwner = new ResourceOwner
            {
                Id = "id"
            };
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(p => p.GetResourceOwnerByClaim("phone_number", phone)).Returns(() => Task.FromResult(resourceOwner));

            // ACT
            var result = await _smsAuthenticationOperation.Execute(phone);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(resourceOwner.Id, result.Id);
        }

        [Fact]
        public async Task When_AutomaticScimResourceCreation_Is_Enabled_Then_Operation_Is_Called()
        {
            // ARRANGE
            const string phone = "phone";
            var resourceOwner = new ResourceOwner
            {
                Id = "id"
            };
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(p => p.GetResourceOwnerByClaim("phone", phone)).Returns(() => Task.FromResult((ResourceOwner)null));
            _smsAuthenticationOptions.IsScimResourceAutomaticallyCreated = true;
            _smsAuthenticationOptions.AuthenticationOptions = new Basic.BasicAuthenticationOptions
            {
                AuthorizationWellKnownConfiguration = "auth",
                ClientId = "clientid",
                ClientSecret = "clientsecret"
            };
            _smsAuthenticationOptions.ScimBaseUrl = "scim";

            // ACT
            await _smsAuthenticationOperation.Execute(phone);

            // ASSERT
            _userActionsStub.Verify(u => u.AddUser(It.IsAny<AddUserParameter>(), It.IsAny<AuthenticationParameter>(), It.IsAny<string>(), true, null));
        }

        [Fact]
        public async Task When_AutomaticScimResourceCreation_Is_Not_Enabled_Then_Operation_Is_Called()
        {
            // ARRANGE
            const string phone = "phone";
            var resourceOwner = new ResourceOwner
            {
                Id = "id"
            };
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(p => p.GetResourceOwnerByClaim("phone", phone)).Returns(() => Task.FromResult((ResourceOwner)null));
            _smsAuthenticationOptions.IsScimResourceAutomaticallyCreated = false;
            _smsAuthenticationOptions.AuthenticationOptions = new Basic.BasicAuthenticationOptions();

            // ACT
            await _smsAuthenticationOperation.Execute(phone);

            // ASSERT
            _userActionsStub.Verify(u => u.AddUser(It.IsAny<AddUserParameter>(), null, null, false, null));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _userActionsStub = new Mock<IUserActions>();
            _smsAuthenticationOptions = new SmsAuthenticationOptions();
            _smsAuthenticationOperation = new SmsAuthenticationOperation(_resourceOwnerRepositoryStub.Object, _userActionsStub.Object, _smsAuthenticationOptions);
        }
    }
}
