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
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public sealed class LocalOpenIdUserAuthenticationActionFixture
    {
        private Mock<IAuthenticateResourceOwnerService> _authenticateResourceOwnerServiceStub;
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryFake;
        private Mock<IAuthenticateHelper> _authenticateHelperFake;
        private ILocalOpenIdUserAuthenticationAction _localUserAuthenticationAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _localUserAuthenticationAction.Execute(null, null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _localUserAuthenticationAction.Execute(localAuthenticationParameter, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Resource_Owner_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();
            var authorizationParameter = new AuthorizationParameter();
            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwnerAsync(It.IsAny<string>(),
                It.IsAny<string>())).Returns(Task.FromResult((ResourceOwner)null));

            // ACT & ASSERT
            await Assert.ThrowsAsync<IdentityServerAuthenticationException>(() => _localUserAuthenticationAction.Execute(localAuthenticationParameter, authorizationParameter, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Resource_Owner_Credentials_Are_Correct_Then_Event_Is_Logged_And_Claims_Are_Returned()
        {
            // ARRANGE
            const string subject = "subject";
            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();
            var authorizationParameter = new AuthorizationParameter();
            var resourceOwner = new ResourceOwner
            {
                Id = subject
            };
            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwnerAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(resourceOwner));

            // ACT
            var result = await _localUserAuthenticationAction.Execute(localAuthenticationParameter,
                authorizationParameter, 
                null).ConfigureAwait(false);

            // Specify the resource owner authentication date
            Assert.NotNull(result);
            Assert.NotNull(result.Claims);
            Assert.True(result.Claims.Any(r => r.Type == ClaimTypes.AuthenticationInstant ||
                r.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
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
