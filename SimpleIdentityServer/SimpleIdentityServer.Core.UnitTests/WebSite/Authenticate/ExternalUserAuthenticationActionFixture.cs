using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using NUnit.Framework;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    [TestFixture]
    public sealed class ExternalUserAuthenticationActionFixture
    {
        private IExternalUserAuthenticationAction _externalUserAuthenticationAction;

        [Test]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claims = new List<Claim>
            {
                new Claim("sub", "sub")
            };

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _externalUserAuthenticationAction.Execute(null, null));
            Assert.Throws<ArgumentNullException>(() => _externalUserAuthenticationAction.Execute(claims, null));
        }

        [Test]
        public void When_Passing_Not_Supported_Provider_Type_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claims = new List<Claim>
            {
                new Claim("sub", "sub")
            };
            const string providerType = "not_supported";

            // ACTS & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => _externalUserAuthenticationAction.Execute(claims, providerType));
            Assert.IsTrue(exception.Code == ErrorCodes.UnhandledExceptionCode);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.TheExternalProviderIsNotSupported, providerType));
        }

        [Test]
        public void When_Passing_Parameters_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string subject = "subject";
            var claims = new List<Claim>
            {
                new Claim(Constants.MicrosoftClaimNames.Id, subject)
            };

            // ACT
            var result = _externalUserAuthenticationAction.Execute(claims, Constants.ProviderTypeNames.Microsoft);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(result.First().Type == Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
            Assert.IsTrue(result.First().Value == subject);
        }

        private void InitializeFakeObjects()
        {
            _externalUserAuthenticationAction = new ExternalUserAuthenticationAction();   
        }
    }
}
