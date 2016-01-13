using Moq;
using NUnit.Framework;
using SimpleIdentityServer.Core.Api.Registration.Actions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Core.UnitTests.Api.Registration
{
    [TestFixture]
    public sealed class RegisterClientActionFixture
    {
        private Mock<IRegistrationParameterValidator> _registrationParameterValidatorFake;

        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceFake;

        private IRegisterClientAction _registerClientAction;

        [Test]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _registerClientAction.Execute(null));
        }

        [Test]
        public void When_Passing_Registration_Parameter_Without_Specific_Values_Then_Client_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientName = "client_name";
            var registrationParameter = new RegistrationParameter
            {
                ClientName = clientName
            };

            // ACT
            var client = _registerClientAction.Execute(registrationParameter);

            // ASSERT
            _registrationParameterValidatorFake.Verify(r => r.Validate(registrationParameter));
            _simpleIdentityServerEventSourceFake.Verify(s => s.StartRegistration(clientName));
            Assert.IsTrue(client.ResponseTypes.Contains(ResponseType.code));
            Assert.IsTrue(client.GrantTypes.Contains(GrantType.authorization_code));
            Assert.IsTrue(client.ApplicationType == ApplicationTypes.web);
        }

        [Test]
        public void When_Passing_Registration_Parameter_With_Specific_Values_Then_Client_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientName = "client_name";
            var registrationParameter = new RegistrationParameter
            {
                ClientName = clientName,
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.token
                },
                GrantTypes = new List<GrantType>
                {
                    GrantType.@implicit
                },
                ApplicationType = ApplicationTypes.native
            };

            // ACT
            var client = _registerClientAction.Execute(registrationParameter);

            // ASSERT
            _registrationParameterValidatorFake.Verify(r => r.Validate(registrationParameter));
            _simpleIdentityServerEventSourceFake.Verify(s => s.StartRegistration(clientName));
            Assert.IsTrue(client.ResponseTypes.Contains(ResponseType.token));
            Assert.IsTrue(client.GrantTypes.Contains(GrantType.@implicit));
            Assert.IsTrue(client.ApplicationType == ApplicationTypes.native);
        }

        private void InitializeFakeObjects()
        {
            _registrationParameterValidatorFake = new Mock<IRegistrationParameterValidator>();
            _simpleIdentityServerEventSourceFake = new Mock<ISimpleIdentityServerEventSource>();
            _registerClientAction = new RegisterClientAction(
                _registrationParameterValidatorFake.Object,
                _simpleIdentityServerEventSourceFake.Object);
        }
    }
}
