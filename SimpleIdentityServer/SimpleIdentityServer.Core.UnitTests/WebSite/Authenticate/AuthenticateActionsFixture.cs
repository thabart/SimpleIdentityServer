using Moq;
using NUnit.Framework;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;

using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    [TestFixture]
    public sealed class AuthenticateActionsFixture
    {
        private Mock<IAuthenticateResourceOwnerOpenIdAction> _authenticateResourceOwnerActionFake;

        private Mock<ILocalOpenIdUserAuthenticationAction> _localOpenIdUserAuthenticationActionFake;

        private Mock<IExternalOpenIdUserAuthenticationAction> _externalUserAuthenticationFake;

        private IAuthenticateActions _authenticateActions;

        [Test]
        public void When_Passing_Null_AuthorizationParameter_To_The_Action_AuthenticateResourceOwner_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.AuthenticateResourceOwnerOpenId(null, null, null));
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.AuthenticateResourceOwnerOpenId(authorizationParameter, null, null));
        }

        [Test]
        public void When_Passing_Null_LocalAuthenticateParameter_To_The_Action_LocalUserAuthentication_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();
            List<Claim> claims;

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.LocalOpenIdUserAuthentication(null, null, null, out claims));
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.LocalOpenIdUserAuthentication(localAuthenticationParameter, null, null, out claims));
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
            _authenticateActions.AuthenticateResourceOwnerOpenId(authorizationParameter, claimsPrincipal, null);

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
            _authenticateActions.LocalOpenIdUserAuthentication(localUserAuthentication,
                authorizationParameter, 
                null, 
                out claims);

            // ASSERT
            _localOpenIdUserAuthenticationActionFake.Verify(a => a.Execute(localUserAuthentication, 
                authorizationParameter,
                null,
                out claims));
        }

        [Test]
        public void When_Passing_Null_Parameters_To_The_Action_ExternalUserAuthentication_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claims = new List<Claim>
            {
                new Claim("sub", "subject")
            };
            var authorizationParameter = new AuthorizationParameter();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.ExternalOpenIdUserAuthentication(null, null, null));
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.ExternalOpenIdUserAuthentication(claims, null, null));
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.ExternalOpenIdUserAuthentication(claims, authorizationParameter, null));
        }

        [Test]
        public void When_Passing_Parameters_Needed_To_The_Action_ExternalUserAuthentication_Then_The_Action_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claims = new List<Claim>
            {
                new Claim("sub", "subject")
            };
            var authorizationParameter = new AuthorizationParameter();
            var code = "code";

            // ACT
            _authenticateActions.ExternalOpenIdUserAuthentication(claims, authorizationParameter, code);

            // ASSERT
            _externalUserAuthenticationFake.Verify(a => a.Execute(
                claims,
                authorizationParameter,
                code));
        }

        private void InitializeFakeObjects()
        {
            _authenticateResourceOwnerActionFake = new Mock<IAuthenticateResourceOwnerOpenIdAction>();
            _localOpenIdUserAuthenticationActionFake = new Mock<ILocalOpenIdUserAuthenticationAction>();
            _externalUserAuthenticationFake = new Mock<IExternalOpenIdUserAuthenticationAction>();
            _authenticateActions = new AuthenticateActions(
                _authenticateResourceOwnerActionFake.Object,
                _localOpenIdUserAuthenticationActionFake.Object,
                _externalUserAuthenticationFake.Object);
        }
    }
}
