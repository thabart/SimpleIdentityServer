using NUnit.Framework;

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.UnitTests.Fake;
using SimpleIdentityServer.Core.Validators;
using System.Collections.Generic;

namespace SimpleIdentityServer.Core.UnitTests.Validators
{
    [TestFixture]
    public class ClientValidatorFixture : BaseFixture
    {
        private IClientValidator _clientValidator;

        [Test]
        public void When_Passing_Null_To_ValidateGrantType_Then_False_Is_Returned()
        {
            // ARRANGE
            InitializeMockingObjects();

            // ACT
            var result = _clientValidator.ValidateGrantType(GrantType.authorization_code, null);

            // ASSERTS
            Assert.IsFalse(result);
        }

        [Test]
        public void When_Passing_Client_Without_GrantTypes_Then_The_ClientGrantType_Is_AuthorizationCode()
        {
            // ARRANGE
            InitializeMockingObjects();
            var client = new Client();

            // ACT
            var result = _clientValidator.ValidateGrantType(GrantType.authorization_code, client);

            // ASSERTS
            Assert.IsTrue(result);
        }

        [Test]
        public void When_Passing_Client_With_GrantType_Implicit_Then_TheClient_Contains_Implicit_GrantType()
        {
            // ARRANGE
            InitializeMockingObjects();
            var client = new Client
            {
                GrantTypes = new List<GrantType>
                {
                    GrantType.@implicit
                }
            };

            // ACT
            var result = _clientValidator.ValidateGrantType(GrantType.@implicit, client);

            // ASSERTS
            Assert.IsTrue(result);
        }
        
        public void InitializeMockingObjects()
        {
            var clientRepository = FakeFactories.GetClientRepository();
            _clientValidator = new ClientValidator(clientRepository);
        }
    }
}
