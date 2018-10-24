using Moq;
using SimpleIdentityServer.Authenticate.SMS.Actions;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.WebSite.User;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Authenticate.SMS.Tests.Actions
{
    public class SmsAuthenticationOperationFixture
    {
        private Mock<IGenerateAndSendSmsCodeOperation> _generateAndSendSmsCodeOperationStub;
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IUserActions> _userActionsStub;
        private Mock<ISubjectBuilder> _subjectBuilderStub;
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
            _generateAndSendSmsCodeOperationStub.Verify(s => s.Execute(phone));
            Assert.NotNull(result);
            Assert.Equal(resourceOwner.Id, result.Id);
        }

        [Fact]
        public async Task When_ResourceOwnerDoesntExist_Then_NewOne_Is_Created()
        {
            // ARRANGE
            const string phone = "phone";
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(p => p.GetResourceOwnerByClaim("phone", phone)).Returns(() => Task.FromResult((ResourceOwner)null));
            
            // ACT
            await _smsAuthenticationOperation.Execute(phone);

            // ASSERT
            _generateAndSendSmsCodeOperationStub.Verify(s => s.Execute(phone));
            _userActionsStub.Verify(u => u.AddUser(It.IsAny<AddUserParameter>(), It.IsAny<string>()));
        }

        private void InitializeFakeObjects()
        {
            _generateAndSendSmsCodeOperationStub = new Mock<IGenerateAndSendSmsCodeOperation>();
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _subjectBuilderStub = new Mock<ISubjectBuilder>();
            _userActionsStub = new Mock<IUserActions>();
            _smsAuthenticationOptions = new SmsAuthenticationOptions();
            _smsAuthenticationOperation = new SmsAuthenticationOperation(
                _generateAndSendSmsCodeOperationStub.Object,
                _resourceOwnerRepositoryStub.Object, _userActionsStub.Object, _subjectBuilderStub.Object, _smsAuthenticationOptions);
        }
    }
}
