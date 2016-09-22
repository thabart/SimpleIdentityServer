using System;
using System.Collections.Generic;
using Moq;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.WebSite.Consent;
using SimpleIdentityServer.Core.WebSite.Consent.Actions;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Consent
{
    public sealed class ConsentActionsFixture
    {
        private Mock<IDisplayConsentAction> _displayConsentActionFake;

        private Mock<IConfirmConsentAction> _confirmConsentActionFake;

        private IConsentActions _consentActions;

        [Fact]
        public void When_Passing_Null_Parameter_To_DisplayConsent_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();
            Models.Client client;
            List<Scope> scopes;
            List<string> allowedClaims;

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(
                () => _consentActions.DisplayConsent(null, null, out client, out scopes, out allowedClaims));
            Assert.Throws<ArgumentNullException>(
                () => _consentActions.DisplayConsent(authorizationParameter, null, out client, out scopes, out allowedClaims));
        }

        [Fact]
        public void When_Passing_Null_Parameter_To_ConfirmConsent_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(
                () => _consentActions.ConfirmConsent(null, null));
            Assert.Throws<ArgumentNullException>(
                () => _consentActions.ConfirmConsent(authorizationParameter, null));
        }

        private void InitializeFakeObjects()
        {
            _displayConsentActionFake = new Mock<IDisplayConsentAction>();
            _confirmConsentActionFake = new Mock<IConfirmConsentAction>();
            _consentActions = new ConsentActions(_displayConsentActionFake.Object,
                _confirmConsentActionFake.Object);
        }
    }
}
