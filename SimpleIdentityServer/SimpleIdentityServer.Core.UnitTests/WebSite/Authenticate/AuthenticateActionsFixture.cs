using System.Linq;
using Moq;
using NUnit.Framework;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Errors;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    [TestFixture]
    public sealed class AuthenticateActionsFixture
    {
        private Mock<IAuthenticateResourceOwnerOpenIdAction> _authenticateResourceOwnerActionFake;

        private Mock<ILocalOpenIdUserAuthenticationAction> _localOpenIdUserAuthenticationActionFake;

        private Mock<IExternalOpenIdUserAuthenticationAction> _externalUserAuthenticationFake;

        private Mock<ILocalUserAuthenticationAction> _localUserAuthenticationActionFake;

        private Mock<IExternalUserAuthenticationAction> _externalUserAuthenticationActionFake;

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
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.ExternalOpenIdUserAuthentication(null, null, null, null));
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.ExternalOpenIdUserAuthentication(claims, null, null, null));
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.ExternalOpenIdUserAuthentication(claims, authorizationParameter, null, null));
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.ExternalOpenIdUserAuthentication(claims, authorizationParameter, "code", null));
        }

        [Test]
        public void When_Passing_Not_Supported_Provider_Type_To_The_Action_ExternalUserAuthentication_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claims = new List<Claim>
            {
                new Claim("sub", "subject")
            };
            var authorizationParameter = new AuthorizationParameter();
            var code = "code";
            const string providerType = "not_supported_provider_type";

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => 
                _authenticateActions.ExternalOpenIdUserAuthentication(claims, authorizationParameter, code, providerType));
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.TheExternalProviderIsNotSupported, providerType));
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
            _authenticateActions.ExternalOpenIdUserAuthentication(claims, authorizationParameter, code, Constants.ProviderTypeNames.Microsoft);

            // ASSERT
            _externalUserAuthenticationFake.Verify(a => a.Execute(
                claims,
                authorizationParameter,
                code, 
                Constants.ProviderTypeNames.Microsoft));
        }

        [Test]
        public void When_Passing_Null_Parameter_To_LocalUserAuthentication_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.LocalUserAuthentication(null));
        }

        [Test]
        public void When_Passing_Needed_Parameter_To_LocalUserAuthentication_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string claimType = "sub";
            const string claimValue = "subject";
            var localAuthenticationParameter = new LocalAuthenticationParameter
            {
                Password = "password",
                UserName = "username"
            };
            var claims = new List<Claim>
            {
                new Claim(claimType, claimValue)
            };
            _localUserAuthenticationActionFake.Setup(l => l.Execute(It.IsAny<LocalAuthenticationParameter>()))
                .Returns(claims);

            // ACT
            var result = _authenticateActions.LocalUserAuthentication(localAuthenticationParameter);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(result.First().Type == claimType && result.First().Value == claimValue);
        }

        [Test]
        public void When_Passing_Null_Parameter_To_ExternalUserAuthentication_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claims = new List<Claim>
            {
                new Claim("sub", "sub")
            };

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.ExternalUserAuthentication(null, null));
            Assert.Throws<ArgumentNullException>(() => _authenticateActions.ExternalUserAuthentication(claims, null));
        }

        [Test]
        public void When_Passing_Needed_Parameters_To_ExternalUserAuthentication_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claims = new List<Claim>
            {
                new Claim("sub", "sub")
            };

            // ACT
            _authenticateActions.ExternalUserAuthentication(claims, "provider_type");

            // ASSERT
            _externalUserAuthenticationActionFake.Verify(e => e.Execute(It.IsAny<List<Claim>>(),
                It.IsAny<string>()));
        }

        private void InitializeFakeObjects()
        {
            _authenticateResourceOwnerActionFake = new Mock<IAuthenticateResourceOwnerOpenIdAction>();
            _localOpenIdUserAuthenticationActionFake = new Mock<ILocalOpenIdUserAuthenticationAction>();
            _externalUserAuthenticationFake = new Mock<IExternalOpenIdUserAuthenticationAction>();
            _localUserAuthenticationActionFake = new Mock<ILocalUserAuthenticationAction>();
            _externalUserAuthenticationActionFake = new Mock<IExternalUserAuthenticationAction>();
            _authenticateActions = new AuthenticateActions(
                _authenticateResourceOwnerActionFake.Object,
                _localOpenIdUserAuthenticationActionFake.Object,
                _externalUserAuthenticationFake.Object,
                _localUserAuthenticationActionFake.Object,
                _externalUserAuthenticationActionFake.Object);
        }
    }
}
