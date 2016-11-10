using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Moq;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public sealed class LocalOpenIdUserAuthenticationActionFixture
    {
        private Mock<IAuthenticateResourceOwnerService> _authenticateResourceOwnerServiceStub;
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryFake;
        private Mock<IAuthenticateHelper> _authenticateHelperFake;
        private ILocalOpenIdUserAuthenticationAction _localUserAuthenticationAction;

        [Fact]
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

        [Fact]
        public void When_Resource_Owner_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();
            var authorizationParameter = new AuthorizationParameter();
            List<Claim> claims;
            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwner(It.IsAny<string>(),
                It.IsAny<string>())).Returns((ResourceOwner)null);

            // ACT & ASSERT
            Assert.Throws<IdentityServerAuthenticationException>(
                () => _localUserAuthenticationAction.Execute(localAuthenticationParameter, authorizationParameter, null, out claims));
        }

        [Fact]
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
            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwner(It.IsAny<string>(),
                It.IsAny<string>())).Returns(resourceOwner);

            // ACT
            _localUserAuthenticationAction.Execute(localAuthenticationParameter,
                authorizationParameter, 
                null, 
                out returnedClaims);
            
            // Specify the resource owner authentication date
            Assert.True(returnedClaims.Any(r => r.Type == ClaimTypes.AuthenticationInstant || 
                r.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
        }

        private void InitializeFakeObjects()
        {
            _authenticateResourceOwnerServiceStub = new Mock<IAuthenticateResourceOwnerService>();
            _resourceOwnerRepositoryFake = new Mock<IResourceOwnerRepository>();
            _authenticateHelperFake = new Mock<IAuthenticateHelper>();
            _localUserAuthenticationAction = new LocalOpenIdUserAuthenticationAction(
                _authenticateResourceOwnerServiceStub.Object,
                _resourceOwnerRepositoryFake.Object,
                _authenticateHelperFake.Object);
        }
    }
}
