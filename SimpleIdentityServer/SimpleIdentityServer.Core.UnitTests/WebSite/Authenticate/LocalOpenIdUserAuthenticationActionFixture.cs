using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Moq;

using NUnit.Framework;

using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    [TestFixture]
    public sealed class LocalOpenIdUserAuthenticationActionFixture
    {

        private Mock<IResourceOwnerService> _resourceOwnerServiceFake;

        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryFake;

        private Mock<IAuthenticateHelper> _authenticateHelperFake;

        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceFake;

        private ILocalOpenIdUserAuthenticationAction _localUserAuthenticationAction;

        [Test]
        public void When_Passing_Null_Parameter_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();
            List<Claim> claims;

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(
                () => _localUserAuthenticationAction.Execute(null, null, null, out claims));
            Assert.Throws<ArgumentNullException>(
                () => _localUserAuthenticationAction.Execute(localAuthenticationParameter, null, null, out claims));
        }

        [Test]
        public void When_Resource_Owner_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();
            var authorizationParameter = new AuthorizationParameter();
            List<Claim> claims;
            _resourceOwnerServiceFake.Setup(r => r.Authenticate(It.IsAny<string>(),
                It.IsAny<string>())).Returns(string.Empty);

            // ACT & ASSERT
            Assert.Throws<IdentityServerAuthenticationException>(
                () => _localUserAuthenticationAction.Execute(localAuthenticationParameter, authorizationParameter, null, out claims));
        }

        [Test]
        public void When_Resource_Owner_Credentials_Are_Correct_Then_Event_Is_Logged_And_Claims_Are_Returned()
        {
            // ARRANGE
            const string subject = "subject";
            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();
            var authorizationParameter = new AuthorizationParameter();
            List<Claim> returnedClaims;
            var resourceOwner = new ResourceOwner
            {
                Id = subject
            };
            _resourceOwnerServiceFake.Setup(r => r.Authenticate(It.IsAny<string>(),
                It.IsAny<string>())).Returns(subject);
            _resourceOwnerRepositoryFake.Setup(r => r.GetBySubject(It.IsAny<string>()))
                .Returns(resourceOwner);

            // ACT
            _localUserAuthenticationAction.Execute(localAuthenticationParameter,
                authorizationParameter, 
                null, 
                out returnedClaims);

            // ASSERT
            _simpleIdentityServerEventSourceFake.Verify(s => s.AuthenticateResourceOwner(subject));
            
            // Specify the resource owner authentication date
            Assert.IsTrue(returnedClaims.Any(r => r.Type == ClaimTypes.AuthenticationInstant || 
                r.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerServiceFake = new Mock<IResourceOwnerService>();
            _resourceOwnerRepositoryFake = new Mock<IResourceOwnerRepository>();
            _authenticateHelperFake = new Mock<IAuthenticateHelper>();
            _simpleIdentityServerEventSourceFake = new Mock<ISimpleIdentityServerEventSource>();
            _localUserAuthenticationAction = new LocalOpenIdUserAuthenticationAction(
                _resourceOwnerServiceFake.Object,
                _resourceOwnerRepositoryFake.Object,
                _authenticateHelperFake.Object,
                _simpleIdentityServerEventSourceFake.Object);
        }
    }
}
