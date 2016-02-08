using System;
using System.Collections.Generic;
using System.Security.Claims;
using Moq;
using NUnit.Framework;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    [TestFixture]
    public sealed class ExternalOpenIdUserAuthenticationActionFixture
    {
        private Mock<IAuthenticateHelper> _authenticateHelperStub;

        private IExternalOpenIdUserAuthenticationAction _externalOpenIdUserAuthenticationAction;

        [Test]
        public void When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claims = new List<Claim>
            {
                new Claim("sub", "subject")
            };
            var authorizationParameter = new AuthorizationParameter();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _externalOpenIdUserAuthenticationAction.Execute(null, null, null));
            Assert.Throws<ArgumentNullException>(() => _externalOpenIdUserAuthenticationAction.Execute(claims, null, null));
            Assert.Throws<ArgumentNullException>(() => _externalOpenIdUserAuthenticationAction.Execute(claims, authorizationParameter, null));
        }

        [Test]
        public void When_Passing_Parameters_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claims = new List<Claim>
            {
                new Claim("sub", "subject")
            };
            var authorizationParameter = new AuthorizationParameter();
            var code = "code";
            var actionResult = new ActionResult
            {
                Type = TypeActionResult.None
            };
            _authenticateHelperStub.Setup(a => a.ProcessRedirection(It.IsAny<AuthorizationParameter>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<Claim>>())).Returns(actionResult);

            // ACT
            var result = _externalOpenIdUserAuthenticationAction.Execute(claims, authorizationParameter, code);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Type == TypeActionResult.None);
        }

        private void InitializeFakeObjects()
        {
            _authenticateHelperStub = new Mock<IAuthenticateHelper>();
            _externalOpenIdUserAuthenticationAction = new ExternalOpenIdUserAuthenticationAction(
                _authenticateHelperStub.Object);
        }
    }
}
