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

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public sealed class AuthenticateHelperFixture
    {
        private Mock<IParameterParserHelper> _parameterParserHelperFake;

        private Mock<IActionResultFactory> _actionResultFactoryFake;

        private Mock<IConsentHelper> _consentHelperFake;

        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;

        private IAuthenticateHelper _authenticateHelper;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            
            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _authenticateHelper.ProcessRedirection(null, null, null, null));
        }

        [Fact]
        public void When_PromptConsent_Parameter_Is_Passed_Then_Redirect_To_ConsentScreen()
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
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirection())
                .Returns(actionResult);
            _parameterParserHelperFake.Setup(p => p.ParsePromptParameters(It.IsAny<string>()))
                .Returns(prompts);

            // ACT
            actionResult = _authenticateHelper.ProcessRedirection(authorizationParameter,
                code,
                subject,
                claims);

            // ASSERTS
            Assert.True(actionResult.RedirectInstruction.Action == IdentityServerEndPoints.ConsentIndex);
            Assert.True(actionResult.RedirectInstruction.Parameters.Any(p => p.Name == code && p.Value == code));
        }

        [Fact]
        public void When_Consent_Has_Already_Been_Given_Then_Redirect_To_Callback()
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
            _parameterParserHelperFake.Setup(p => p.ParsePromptParameters(It.IsAny<string>()))
                .Returns(prompts);
            _consentHelperFake.Setup(c => c.GetConsentConfirmedByResourceOwner(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(consent);
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl())
                .Returns(actionResult);

            // ACT
            actionResult = _authenticateHelper.ProcessRedirection(authorizationParameter,
                code,
                subject,
                claims);

            // ASSERTS
            Assert.True(actionResult.RedirectInstruction.ResponseMode == ResponseMode.form_post);
        }

        [Fact]
        public void When_There_Is_No_Consent_Then_Redirect_To_Consent_Screen()
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
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirection())
                .Returns(actionResult);
            _parameterParserHelperFake.Setup(p => p.ParsePromptParameters(It.IsAny<string>()))
                .Returns(prompts);
            _consentHelperFake.Setup(c => c.GetConsentConfirmedByResourceOwner(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>())).Returns(() => null);

            // ACT
            actionResult = _authenticateHelper.ProcessRedirection(authorizationParameter,
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
            _authenticateHelper = new AuthenticateHelper(
                _parameterParserHelperFake.Object,
                _actionResultFactoryFake.Object,
                _consentHelperFake.Object,
                _generateAuthorizationResponseFake.Object);
        }
    }
}
