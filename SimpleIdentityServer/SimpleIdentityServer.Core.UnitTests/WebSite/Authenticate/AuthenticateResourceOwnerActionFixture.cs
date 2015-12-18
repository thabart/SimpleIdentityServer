using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using NUnit.Framework;

using Moq;

using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    [TestFixture]
    public sealed class AuthenticateResourceOwnerActionFixture
    {
        private Mock<IParameterParserHelper> _parameterParserHelperFake;

        private Mock<IActionResultFactory> _actionResultFactoryFake;

        private Mock<IAuthenticateHelper> _authenticateHelperFake;

        private IAuthenticateResourceOwnerAction _authenticateResourceOwnerAction;

        [Test]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _authenticateResourceOwnerAction.Execute(null, null, null));
        }

        [Test]
        public void When_No_Resource_Owner_Is_Passed_Then_Redirect_To_Index_Page()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT
            _authenticateResourceOwnerAction.Execute(authorizationParameter, null, null);

            // ASSERT
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithNoEffect());
        }

        [Test]
        public void When_Resource_Owner_Is_Not_Authenticated_Then_Redirect_To_Index_Page()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // ACT
            _authenticateResourceOwnerAction.Execute(authorizationParameter, 
                claimsPrincipal, 
                null);

            // ASSERT
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithNoEffect());
        }

        [Test]
        public void When_Prompt_Parameter_Contains_Login_Value_Then_Redirect_To_Index_Page()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();
            var claimsIdentity = new ClaimsIdentity("identityServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var promptParameters = new List<PromptParameter>
            {
                PromptParameter.login
            };
            _parameterParserHelperFake.Setup(p => p.ParsePromptParameters(It.IsAny<string>()))
                .Returns(promptParameters);

            // ACT
            _authenticateResourceOwnerAction.Execute(authorizationParameter,
                claimsPrincipal,
                null);

            // ASSERT
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithNoEffect());
        }

        [Test]
        public void When_Prompt_Parameter_Doesnt_Contain_Login_Value_And_Resource_Owner_Is_Authenticated_Then_Helper_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string code = "code";
            const string subject = "subject";
            var authorizationParameter = new AuthorizationParameter();
            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "identityServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var promptParameters = new List<PromptParameter>
            {
                PromptParameter.consent
            };
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            _parameterParserHelperFake.Setup(p => p.ParsePromptParameters(It.IsAny<string>()))
                .Returns(promptParameters);
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirection())
                .Returns(actionResult);

            // ACT
            _authenticateResourceOwnerAction.Execute(authorizationParameter,
                claimsPrincipal,
                code);

            // ASSERT
            _authenticateHelperFake.Verify(a => a.ProcessRedirection(authorizationParameter, 
                code, 
                subject,
                It.IsAny<List<Claim>>()));
        }

        private void InitializeFakeObjects()
        {
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _actionResultFactoryFake = new Mock<IActionResultFactory>();
            _authenticateHelperFake = new Mock<IAuthenticateHelper>();
            _authenticateResourceOwnerAction = new AuthenticateResourceOwnerAction(
                _parameterParserHelperFake.Object,
                _actionResultFactoryFake.Object,
                _authenticateHelperFake.Object);
        }
    }
}
