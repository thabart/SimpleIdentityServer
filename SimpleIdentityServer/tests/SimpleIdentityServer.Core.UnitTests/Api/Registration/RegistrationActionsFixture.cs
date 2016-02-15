using System;
using Moq;
using SimpleIdentityServer.Core.Api.Registration;
using SimpleIdentityServer.Core.Api.Registration.Actions;
using SimpleIdentityServer.Core.Parameters;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Registration
{
    public sealed class RegistrationActionsFixture
    {
        private Mock<IRegisterClientAction> _registerClientActionFake;

        private IRegistrationActions _registrationActions;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _registrationActions.PostRegistration(null));
        }

        [Fact]
        public void When_Passing_Valid_Parameter_Then_Action_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            _registrationActions.PostRegistration(new RegistrationParameter());

            // ASSERT
            _registerClientActionFake.Verify(r => r.Execute(It.IsAny<RegistrationParameter>()));
        }

        private void InitializeFakeObjects()
        {
            _registerClientActionFake = new Mock<IRegisterClientAction>();
            _registrationActions = new RegistrationActions(_registerClientActionFake.Object);
        }
    }
}
