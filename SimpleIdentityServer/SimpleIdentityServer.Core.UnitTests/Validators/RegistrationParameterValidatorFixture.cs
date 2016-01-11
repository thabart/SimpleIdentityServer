using System;
using System.Collections.Generic;
using NUnit.Framework;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.UnitTests.Validators
{
    [TestFixture]
    public sealed class RegistrationParameterValidatorFixture
    {
        private IRegistrationParameterValidator _registrationParameterValidator;

        [Test]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _registrationParameterValidator.Validate(null));
        }

        [Test]
        public void When_There_Is_No_Request_Uri_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = null
            };
            
            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidRedirectUri);
            Assert.IsTrue(ex.Message == string.Format(ErrorDescriptions.MissingParameter, Constants.StandardRegistrationRequestParameterNames.RequestUris));
        }

        [Test]
        public void When_One_Request_Uri_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "invalid"
                }
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidRedirectUri);
            Assert.IsTrue(ex.Message == ErrorDescriptions.TheRedirectUriParameterIsNotValid);
        }

        private void InitializeFakeObjects()
        {
            _registrationParameterValidator = new RegistrationParameterValidator();
        }
    }
}
