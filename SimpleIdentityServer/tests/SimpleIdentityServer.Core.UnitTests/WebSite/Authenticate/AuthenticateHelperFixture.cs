using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Moq;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using Xunit;
using System.Threading.Tasks;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Errors;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public sealed class AuthenticateHelperFixture
    {
        private Mock<IParameterParserHelper> _parameterParserHelperFake;
        private Mock<IActionResultFactory> _actionResultFactoryFake;
        private Mock<IConsentHelper> _consentHelperFake;
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;
        private Mock<IClientRepository> _clientRepositoryStub;
        private IAuthenticateHelper _authenticateHelper;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            
            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _authenticateHelper.ProcessRedirection(null, null, null, null));
        }

        [Fact]
        public async Task When_Client_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((Client)null));
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = "client_id"
            };

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _authenticateHelper.ProcessRedirection(authorizationParameter, null, null, null));
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClientIdDoesntExist, authorizationParameter.ClientId));
        }

        [Fact]
        public async Task When_PromptConsent_Parameter_Is_Passed_Then_Redirect_To_ConsentScreen()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string subject = "subject";
            const string code = "code";
            var prompts = new List<PromptParameter>
            {
                PromptParameter.consent
            };
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var authorizationParameter = new AuthorizationParameter();
            var claims = new List<Claim>();
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new Client()));
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirection())
                .Returns(actionResult);
            _parameterParserHelperFake.Setup(p => p.ParsePrompts(It.IsAny<string>()))
                .Returns(prompts);

            // ACT
            actionResult = await _authenticateHelper.ProcessRedirection(authorizationParameter,
                code,
                subject,
                claims);

            // ASSERTS
            Assert.True(actionResult.RedirectInstruction.Action == IdentityServerEndPoints.ConsentIndex);
            Assert.True(actionResult.RedirectInstruction.Parameters.Any(p => p.Name == code && p.Value == code));
        }

        [Fact]
        public async Task When_Consent_Has_Already_Been_Given_Then_Redirect_To_Callback()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string subject = "subject";
            const string code = "code";
            var prompts = new List<PromptParameter>
            {
                PromptParameter.none
            };
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var consent = new Core.Models.Consent();
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseMode = ResponseMode.form_post
            };
            var claims = new List<Claim>();
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new Client()));
            _parameterParserHelperFake.Setup(p => p.ParsePrompts(It.IsAny<string>()))
                .Returns(prompts);
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(consent));
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl())
                .Returns(actionResult);

            // ACT
            actionResult = await _authenticateHelper.ProcessRedirection(authorizationParameter,
                code,
                subject,
                claims);

            // ASSERTS
            Assert.True(actionResult.RedirectInstruction.ResponseMode == ResponseMode.form_post);
        }

        [Fact]
        public async Task When_There_Is_No_Consent_Then_Redirect_To_Consent_Screen()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string subject = "subject";
            const string code = "code";
            var prompts = new List<PromptParameter>
            {
                PromptParameter.none
            };
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var authorizationParameter = new AuthorizationParameter();
            var claims = new List<Claim>();
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new Client()));
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirection())
                .Returns(actionResult);
            _parameterParserHelperFake.Setup(p => p.ParsePrompts(It.IsAny<string>()))
                .Returns(prompts);
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>())).Returns(() => Task.FromResult((Models.Consent)null));

            // ACT
            actionResult = await _authenticateHelper.ProcessRedirection(authorizationParameter,
                code,
                subject,
                claims);

            // ASSERTS
            Assert.True(actionResult.RedirectInstruction.Action == IdentityServerEndPoints.ConsentIndex);
            Assert.True(actionResult.RedirectInstruction.Parameters.Any(p => p.Name == code && p.Value == code));
        }

        private void InitializeFakeObjects()
        {
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _actionResultFactoryFake = new Mock<IActionResultFactory>();
            _consentHelperFake = new Mock<IConsentHelper>();
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _clientRepositoryStub = new Mock<IClientRepository>();
            _authenticateHelper = new AuthenticateHelper(
                _parameterParserHelperFake.Object,
                _actionResultFactoryFake.Object,
                _consentHelperFake.Object,
                _generateAuthorizationResponseFake.Object,
                _clientRepositoryStub.Object);
        }
    }
}
