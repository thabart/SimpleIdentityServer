using Moq;
using NUnit.Framework;

using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.WebSite.Consent.Actions;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.Api.UnitTests.WebSite.Consent
{
    [TestFixture]
    public sealed class ConfirmConsentFixture
    {
        private Mock<IConsentRepository> _consentRepositoryFake;

        private Mock<IClientRepository> _clientRepositoryFake;

        private Mock<IScopeRepository> _scopeRepositoryFake;

        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryFake;

        private Mock<IParameterParserHelper> _parameterParserHelperFake;

        private Mock<IActionResultFactory> _actionResultFactoryFake;

        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;

        private Mock<IConsentHelper> _consentHelperFake;

        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSource;

        private IConfirmConsentAction _confirmConsentAction;

        [Test]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _confirmConsentAction.Execute(null, null));
            Assert.Throws<ArgumentNullException>(() => _confirmConsentAction.Execute(authorizationParameter, null));
        }

        [Test]
        public void When_No_Consent_Has_Been_Given_Then_Create_And_Insert_A_New_One()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string subject = "subject";
            var authorizationParameter = new AuthorizationParameter
            {
                Claims = null,
                Scope = "profile"
            };
            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "SimpleIdentityServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var client = new Client
            {
                ClientId = "clientId"
            };
            var resourceOwner = new ResourceOwner
            {
                Id = subject
            };
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            _consentHelperFake.Setup(c => c.GetConsentConfirmedByResourceOwner(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(() => null);
            _clientRepositoryFake.Setup(c => c.GetClientById(It.IsAny<string>()))
                .Returns(client);
            _parameterParserHelperFake.Setup(p => p.ParseScopeParameters(It.IsAny<string>()))
                .Returns(new List<string>());
            _resourceOwnerRepositoryFake.Setup(r => r.GetBySubject(It.IsAny<string>()))
                .Returns(resourceOwner);
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl())
                .Returns(actionResult);

            // ACT
            _confirmConsentAction.Execute(authorizationParameter, claimsPrincipal);

            // ASSERT
            _consentRepositoryFake.Verify(c => c.InsertConsent(It.IsAny<Core.Models.Consent>()));
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl());
        }

        private void InitializeFakeObjects()
        {
            _consentRepositoryFake = new Mock<IConsentRepository>();
            _clientRepositoryFake = new Mock<IClientRepository>();
            _scopeRepositoryFake = new Mock<IScopeRepository>();
            _resourceOwnerRepositoryFake = new Mock<IResourceOwnerRepository>();
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _actionResultFactoryFake = new Mock<IActionResultFactory>();
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _consentHelperFake = new Mock<IConsentHelper>();
            _simpleIdentityServerEventSource = new Mock<ISimpleIdentityServerEventSource>();
            _confirmConsentAction = new ConfirmConsentAction(
                _consentRepositoryFake.Object,
                _clientRepositoryFake.Object,
                _scopeRepositoryFake.Object,
                _resourceOwnerRepositoryFake.Object,
                _parameterParserHelperFake.Object,
                _actionResultFactoryFake.Object,
                _generateAuthorizationResponseFake.Object,
                _consentHelperFake.Object,
                _simpleIdentityServerEventSource.Object);
        }
    }
}
