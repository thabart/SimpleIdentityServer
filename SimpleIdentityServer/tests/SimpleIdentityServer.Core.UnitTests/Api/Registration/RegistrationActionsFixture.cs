using System;
using Moq;
using SimpleIdentityServer.Core.Api.Registration;
using SimpleIdentityServer.Core.Api.Registration.Actions;
using SimpleIdentityServer.Core.Parameters;
using Xunit;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.UnitTests.Api.Registration
{
    public sealed class RegistrationActionsFixture
    {
        private Mock<IRegisterClientAction> _registerClientActionFake;

        private IRegistrationActions _registrationActions;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _registrationActions.PostRegistration(null));
        }

        [Fact]
        public async Task When_Passing_Valid_Parameter_Then_Action_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            await _registrationActions.PostRegistration(new RegistrationParameter());

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
