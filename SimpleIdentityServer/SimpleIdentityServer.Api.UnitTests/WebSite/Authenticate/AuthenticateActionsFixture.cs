using Moq;
using NUnit.Framework;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;

using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.Api.UnitTests.WebSite.Authenticate
{
    [TestFixture]
    public sealed class AuthenticateActionsFixture
    {
        private Mock<IAuthenticateResourceOwnerAction> _authenticateResourceOwnerActionFake;

        private Mock<ILocalUserAuthenticationAction> _localUserAuthenticationActionFake;

        private IAuthenticateActions _authenticateActions;

        [Test]
        public void When_Passing_Null_AuthorizationParameter_To_The_Action_AuthenticateResourceOwner_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.AuthenticateResourceOwner(null, null, null));
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.AuthenticateResourceOwner(authorizationParameter, null, null));
        }

        [Test]
        public void When_Passing_Null_LocalAuthenticateParameter_To_The_Action_LocalUserAuthentication_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();
            List<Claim> claims;

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.LocalUserAuthentication(null, null, null, out claims));
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.LocalUserAuthentication(localAuthenticationParameter, null, null, out claims));
        }

        [Test]
        public void When_Passing_Parameters_Needed_To_The_Action_AuthenticateResourceOwner_Then_The_Action_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = "clientId"
            };
            var claimsPrincipal = new ClaimsPrincipal();

            // ACT
            _authenticateActions.AuthenticateResourceOwner(authorizationParameter, claimsPrincipal, null);

            // ASSERT
            _authenticateResourceOwnerActionFake.Verify(a => a.Execute(authorizationParameter, claimsPrincipal, null));
        }

        [Test]
        public void When_Passing_Parameters_Needed_To_The_Action_LocalUserAuthentication_Then_The_Action_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = "clientId"
            };
            var localUserAuthentication = new LocalAuthenticationParameter
            {
                UserName = "userName"
            };
            List<Claim> claims;


            // ACT
            _authenticateActions.LocalUserAuthentication(localUserAuthentication,
                authorizationParameter, 
                null, 
                out claims);

            // ASSERT
            _localUserAuthenticationActionFake.Verify(a => a.Execute(localUserAuthentication, 
                authorizationParameter,
                null,
                out claims));
        }

        private void InitializeFakeObjects()
        {
            _authenticateResourceOwnerActionFake = new Mock<IAuthenticateResourceOwnerAction>();
            _localUserAuthenticationActionFake = new Mock<ILocalUserAuthenticationAction>();
            _authenticateActions = new AuthenticateActions(
                _authenticateResourceOwnerActionFake.Object,
                _localUserAuthenticationActionFake.Object);
        }
    }
}
