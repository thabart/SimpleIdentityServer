using Moq;
using NUnit.Framework;

using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Core.UnitTests.Helpers
{
    [TestFixture]
    public sealed class ConsentHelperFixture
    {
        private Mock<IConsentRepository> _consentRepositoryFake;

        private Mock<IParameterParserHelper> _parameterParserHelperFake;

        private IConsentHelper _consentHelper;

        [Test]
        public void When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _consentHelper.GetConsentConfirmedByResourceOwner("subject", null));
        }

        [Test]
        public void When_No_Consent_Has_Been_Given_By_The_Resource_Owner_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string subject = "subject";
            var authorizationParameter = new AuthorizationParameter();

            _consentRepositoryFake.Setup(c => c.GetConsentsForGivenUser(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _consentHelper.GetConsentConfirmedByResourceOwner(subject,
                authorizationParameter);

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_A_Consent_Has_Been_Given_For_Claim_Name_Then_Consent_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string subject = "subject";
            const string claimName = "name";
            const string clientId = "clientId";
            var authorizationParameter = new AuthorizationParameter
            {
                Claims = new ClaimsParameter
                {
                    UserInfo = new List<ClaimParameter>
                    {
                        new ClaimParameter
                        {
                            Name = claimName
                        }
                    }
                },
                ClientId = clientId
            };
            var consents = new List<Consent>
            {
                new Consent
                {
                    Claims = new List<string>
                    {
                        claimName
                    },
                    Client = new Client
                    {
                        ClientId = clientId
                    }
                }
            };

            _consentRepositoryFake.Setup(c => c.GetConsentsForGivenUser(It.IsAny<string>()))
                .Returns(consents);

            // ACT
            var result = _consentHelper.GetConsentConfirmedByResourceOwner(subject,
                authorizationParameter);

            // ASSERT
            Assert.IsNotNull(result);
        }

        private void InitializeFakeObjects()
        {
            _consentRepositoryFake = new Mock<IConsentRepository>();
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _consentHelper = new ConsentHelper(
                _consentRepositoryFake.Object,
                _parameterParserHelperFake.Object);
        }
    }
}
