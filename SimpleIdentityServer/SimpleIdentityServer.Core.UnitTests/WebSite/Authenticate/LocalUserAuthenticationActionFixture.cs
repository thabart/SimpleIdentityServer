using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    [TestFixture]
    public sealed class LocalUserAuthenticationActionFixture
    {
        private Mock<IResourceOwnerService> _resourceOwnerServiceStub;

        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceStub;

        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;

        private ILocalUserAuthenticationAction _localUserAuthenticationAction;

        [Test]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _localUserAuthenticationAction.Execute(null));
        }

        [Test]
        public void When_ResourceOwner_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new LocalAuthenticationParameter
            {
                UserName = "username",
                Password = "password"
            };
            _resourceOwnerServiceStub.Setup(r => r.Authenticate(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerAuthenticationException>(() => _localUserAuthenticationAction.Execute(parameter));
            Assert.IsTrue(exception.Message == ErrorDescriptions.TheResourceOwnerCredentialsAreNotCorrect);
        }

        [Test]
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
            _resourceOwnerServiceStub.Setup(r => r.Authenticate(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns("subject");
            _resourceOwnerRepositoryStub.Setup(r => r.GetBySubject(It.IsAny<string>()))
                .Returns(resourceOwner);

            // ACT
            var claims = _localUserAuthenticationAction.Execute(parameter);

            // ASSERT
            Assert.IsNotNull(claims);
            Assert.IsTrue(claims.Any(c => c.Type == "sub" && c.Value == subject));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerServiceStub = new Mock<IResourceOwnerService>();
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _simpleIdentityServerEventSourceStub = new Mock<ISimpleIdentityServerEventSource>();
            _localUserAuthenticationAction = new LocalUserAuthenticationAction(
                _resourceOwnerServiceStub.Object,
                _simpleIdentityServerEventSourceStub.Object,
                _resourceOwnerRepositoryStub.Object);
            
        }
    }
}
