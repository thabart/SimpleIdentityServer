using System;
using System.Collections.Generic;
using System.Security.Claims;
using Moq;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Repositories;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public sealed class ExternalOpenIdUserAuthenticationActionFixture
    {
        private Mock<IAuthenticateHelper> _authenticateHelperStub;

        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private IExternalOpenIdUserAuthenticationAction _externalOpenIdUserAuthenticationAction;

        [Fact]
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

        [Fact]
        public void When_The_Subject_Is_Not_Specified_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string subject = "subject";
            var claims = new List<Claim>
            {
                new Claim("invalid_subject", subject)
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

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerException>(() => _externalOpenIdUserAuthenticationAction.Execute(claims, authorizationParameter, code));
            Assert.True(exception.Message == ErrorDescriptions.NoSubjectCanBeExtracted);
        }

        [Fact]
        public void When_Passing_Parameters_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string subject = "subject";
            var claims = new List<Claim>
            {
                new Claim("sub", subject)
            };
            var authorizationParameter = new AuthorizationParameter();
            var code = "code";
            var actionResult = new ActionResult
            {
                Type = TypeActionResult.None
            };
            _resourceOwnerRepositoryStub.Setup(r => r.GetBySubject(It.IsAny<string>()))
                .Returns(() => null);
            _authenticateHelperStub.Setup(a => a.ProcessRedirection(It.IsAny<AuthorizationParameter>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<Claim>>())).Returns(actionResult);

            // ACT
            var result = _externalOpenIdUserAuthenticationAction.Execute(claims, authorizationParameter, code);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.Type == TypeActionResult.None);
            _authenticateHelperStub.Verify(a => a.ProcessRedirection(It.IsAny<AuthorizationParameter>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<Claim>>()));
        }

        private void InitializeFakeObjects()
        {
            _authenticateHelperStub = new Mock<IAuthenticateHelper>();
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _externalOpenIdUserAuthenticationAction = new ExternalOpenIdUserAuthenticationAction(
                _authenticateHelperStub.Object,
                _resourceOwnerRepositoryStub.Object);
        }
    }
}
