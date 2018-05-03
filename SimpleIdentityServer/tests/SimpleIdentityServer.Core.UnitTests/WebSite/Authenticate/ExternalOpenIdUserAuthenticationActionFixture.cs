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
using SimpleIdentityServer.Core.Common.Repositories;
using Xunit;
using SimpleIdentityServer.Core.Services;
using System.Threading.Tasks;
using SimpleIdentityServer.Core.Common.Models;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public sealed class ExternalOpenIdUserAuthenticationActionFixture
    {
        private Mock<IAuthenticateHelper> _authenticateHelperStub;
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IAuthenticateResourceOwnerService> _authenticateResourceOwnerServiceStub;
        private Mock<IClaimRepository> _claimRepositoryStub;
        private IExternalOpenIdUserAuthenticationAction _externalOpenIdUserAuthenticationAction;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claims = new List<Claim>
            {
                new Claim("sub", "subject")
            };
            var authorizationParameter = new AuthorizationParameter();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _externalOpenIdUserAuthenticationAction.Execute(null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _externalOpenIdUserAuthenticationAction.Execute(claims, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _externalOpenIdUserAuthenticationAction.Execute(claims, authorizationParameter, null));
        }

        [Fact]
        public async Task When_The_Subject_Is_Not_Specified_Then_Exception_Is_Thrown()
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
                It.IsAny<List<Claim>>())).Returns(Task.FromResult(actionResult));

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _externalOpenIdUserAuthenticationAction.Execute(claims, authorizationParameter, code));
            Assert.True(exception.Message == ErrorDescriptions.NoSubjectCanBeExtracted);
        }

        [Fact]
        public async Task When_Passing_Parameters_Then_Operation_Is_Called()
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
            IEnumerable<ClaimAggregate> claimValues = new List<ClaimAggregate>();
            _authenticateResourceOwnerServiceStub.Setup(r => r.AuthenticateResourceOwnerAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((ResourceOwner)null));
            _claimRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(claimValues));
            _authenticateHelperStub.Setup(a => a.ProcessRedirection(It.IsAny<AuthorizationParameter>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<Claim>>())).Returns(Task.FromResult(actionResult));

            // ACT
            var result = await _externalOpenIdUserAuthenticationAction.Execute(claims, authorizationParameter, code);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ActionResult.Type == TypeActionResult.None);
            _authenticateHelperStub.Verify(a => a.ProcessRedirection(It.IsAny<AuthorizationParameter>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<Claim>>()));
        }

        private void InitializeFakeObjects()
        {
            _authenticateHelperStub = new Mock<IAuthenticateHelper>();
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _authenticateResourceOwnerServiceStub = new Mock<IAuthenticateResourceOwnerService>();
            _claimRepositoryStub = new Mock<IClaimRepository>();
            _externalOpenIdUserAuthenticationAction = new ExternalOpenIdUserAuthenticationAction(
                _authenticateHelperStub.Object,
                _resourceOwnerRepositoryStub.Object,
                _authenticateResourceOwnerServiceStub.Object,
                _claimRepositoryStub.Object);
        }
    }
}
