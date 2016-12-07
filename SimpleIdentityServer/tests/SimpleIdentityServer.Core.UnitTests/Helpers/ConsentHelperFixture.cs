using Moq;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Helpers
{
    public sealed class ConsentHelperFixture
    {
        private Mock<IConsentRepository> _consentRepositoryFake;

        private Mock<IParameterParserHelper> _parameterParserHelperFake;

        private IConsentHelper _consentHelper;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _consentHelper.GetConsentConfirmedByResourceOwnerAsync("subject", null));
        }

        [Fact]
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
            Assert.Null(result);
        }

        [Fact]
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
            IEnumerable<Consent> consents = new List<Consent>
            {
                new Consent
                {
                    Claims = new List<string>
                    {
                        claimName
                    },
                    Client = new Models.Client
                    {
                        ClientId = clientId
                    }
                }
            };

            _consentRepositoryFake.Setup(c => c.GetConsentsForGivenUserAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(consents));

            // ACT
            var result = _consentHelper.GetConsentConfirmedByResourceOwner(subject,
                authorizationParameter);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.Claims.Count == 1);
            Assert.True(result.Claims.First() == claimName);
        }

        [Fact]
        public void When_A_Consent_Has_Been_Given_For_Scope_Profile_Then_Consent_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string subject = "subject";
            const string scope = "profile";
            const string clientId = "clientId";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = scope
            };
            IEnumerable<Consent> consents = new List<Consent>
            {
                new Consent
                {
                    Client = new Models.Client
                    {
                        ClientId = clientId
                    },
                    GrantedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = scope
                        }
                    }
                }
            };
            var scopes = new List<string>
            {
                scope
            };
            _parameterParserHelperFake.Setup(p => p.ParseScopeParameters(It.IsAny<string>())).Returns(scopes);
            _consentRepositoryFake.Setup(c => c.GetConsentsForGivenUserAsync(It.IsAny<string>())).Returns(Task.FromResult(consents));

            // ACT
            var result = _consentHelper.GetConsentConfirmedByResourceOwner(subject,
                authorizationParameter);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.GrantedScopes.Count == 1);
            Assert.True(result.GrantedScopes.First().Name == scope);
        }
        
        [Fact]
        public void When_Consent_Has_Been_Assigned_To_OpenId_Profile_And_Request_Consent_For_Scope_OpenId_Profile_Email_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string subject = "subject";
            const string openIdScope = "openid";
            const string profileScope = "profile";
            const string emailScope = "email";
            const string clientId = "clientId";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = openIdScope + " " + profileScope + " "+ emailScope
            };
            var consents = new List<Consent>
            {
                new Consent
                {
                    Client = new Models.Client
                    {
                        ClientId = clientId
                    },
                    GrantedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = profileScope
                        },
                        new Scope
                        {
                            Name = openIdScope
                        }
                    }
                }
            };
            var scopes = new List<string>
            {
                openIdScope,
                profileScope,
                emailScope
            };

            _parameterParserHelperFake.Setup(p => p.ParseScopeParameters(It.IsAny<string>()))
                .Returns(scopes);
            _consentRepositoryFake.Setup(c => c.GetConsentsForGivenUser(It.IsAny<string>()))
                .Returns(consents);

            // ACT
            var result = _consentHelper.GetConsentConfirmedByResourceOwner(subject,
                authorizationParameter);

            // ASSERT
            Assert.Null(result);
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
