using System;
using System.Collections.Generic;
using NUnit.Framework;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.Models;

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

        [Test]
        public void When_ResponseType_Is_Not_Defined_Then_Set_To_Code()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "http://localhost"
                }
            };

            // ACT
            _registrationParameterValidator.Validate(parameter);

            // ASSERT
            Assert.IsNotNull(parameter);
            Assert.IsTrue(parameter.ResponseTypes.Count == 1);
            Assert.IsTrue(parameter.ResponseTypes.Contains(Core.Models.ResponseType.code));
        }

        [Test]
        public void When_GrantType_Is_Not_Defined_Then_Set_To_Authorization_Code()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "http://localhost"
                }
            };

            // ACT
            _registrationParameterValidator.Validate(parameter);

            // ASSERT
            Assert.IsNotNull(parameter);
            Assert.IsTrue(parameter.GrantTypes.Count == 1);
            Assert.IsTrue(parameter.GrantTypes.Contains(Models.GrantType.authorization_code));
        }

        [Test]
        public void When_Application_Type_Is_Not_Defined_Then_Set_To_Web_Application()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "http://localhost"
                }
            };

            // ACT
            _registrationParameterValidator.Validate(parameter);

            // ASSERT
            Assert.IsNotNull(parameter);
            Assert.IsTrue(parameter.ApplicationType == ApplicationTypes.web);
        }

        private void InitializeFakeObjects()
        {
            _registrationParameterValidator = new RegistrationParameterValidator();
        }
    }
}
