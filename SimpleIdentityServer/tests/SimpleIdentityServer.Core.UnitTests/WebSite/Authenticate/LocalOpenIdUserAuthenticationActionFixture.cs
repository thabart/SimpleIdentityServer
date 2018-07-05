using Moq;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public sealed class LocalOpenIdUserAuthenticationActionFixture
    {
        private Mock<IResourceOwnerAuthenticateHelper> _resourceOwnerAuthenticateHelperStub;
        private Mock<IAuthenticateHelper> _authenticateHelperFake;
        private ILocalOpenIdUserAuthenticationAction _localUserAuthenticationAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _localUserAuthenticationAction.Execute(null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _localUserAuthenticationAction.Execute(localAuthenticationParameter, null, null));
        }

        [Fact]
        public async Task When_Resource_Owner_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();
            var authorizationParameter = new AuthorizationParameter();
            _resourceOwnerAuthenticateHelperStub.Setup(r => r.Authenticate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult((ResourceOwner)null));

            // ACT & ASSERT
            await Assert.ThrowsAsync<IdentityServerAuthenticationException>(() => _localUserAuthenticationAction.Execute(localAuthenticationParameter, authorizationParameter, null));
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
            _resourceOwnerAuthenticateHelperStub.Setup(r => r.Authenticate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(resourceOwner));

            // ACT
            var result = await _localUserAuthenticationAction.Execute(localAuthenticationParameter,
                authorizationParameter, 
                null);

            // Specify the resource owner authentication date
            Assert.NotNull(result);
            Assert.NotNull(result.Claims);
            Assert.True(result.Claims.Any(r => r.Type == ClaimTypes.AuthenticationInstant ||
                r.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerAuthenticateHelperStub = new Mock<IResourceOwnerAuthenticateHelper>();
            _authenticateHelperFake = new Mock<IAuthenticateHelper>();
            _localUserAuthenticationAction = new LocalOpenIdUserAuthenticationAction(
                _resourceOwnerAuthenticateHelperStub.Object,
                _authenticateHelperFake.Object);
        }
    }
}
