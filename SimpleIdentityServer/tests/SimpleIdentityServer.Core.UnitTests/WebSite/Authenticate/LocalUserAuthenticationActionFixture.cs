using Moq;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using System;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public sealed class LocalUserAuthenticationActionFixture
    {
        private Mock<IAuthenticateResourceOwnerService> _authenticateResourceOwnerServiceStub;
        private ILocalUserAuthenticationAction _localUserAuthenticationAction;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _localUserAuthenticationAction.Execute(null));
        }

        [Fact]
        public void When_ResourceOwner_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new LocalAuthenticationParameter
            {
                UserName = "username",
                Password = "password"
            };
            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwner(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns((ResourceOwner)null);

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerAuthenticationException>(() => _localUserAuthenticationAction.Execute(parameter));
            Assert.True(exception.Message == ErrorDescriptions.TheResourceOwnerCredentialsAreNotCorrect);
        }

        [Fact]
        public void When_Authenticate_ResourceOwner_Then_Claims_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string subject = "subject";
            var parameter = new LocalAuthenticationParameter
            {
                UserName = "username",
                Password = "password"
            };
            var resourceOwner = new ResourceOwner
            {
                Id = subject
            };
            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwner(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(resourceOwner);

            // ACT
            var res = _localUserAuthenticationAction.Execute(parameter);

            // ASSERT
            Assert.NotNull(res);
        }

        private void InitializeFakeObjects()
        {
            _authenticateResourceOwnerServiceStub = new Mock<IAuthenticateResourceOwnerService>();
            _localUserAuthenticationAction = new LocalUserAuthenticationAction(_authenticateResourceOwnerServiceStub.Object);
            
        }
    }
}
