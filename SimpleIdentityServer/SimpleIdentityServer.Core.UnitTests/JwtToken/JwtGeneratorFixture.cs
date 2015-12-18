using System.Linq;
using Moq;
using NUnit.Framework;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Encrypt.Encryption;
using SimpleIdentityServer.Core.Jwt.Mapping;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.UnitTests.Fake;
using SimpleIdentityServer.Core.Validators;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using SimpleIdentityServer.DataAccess.Fake;

namespace SimpleIdentityServer.Core.UnitTests.JwtToken
{
    [TestFixture]
    public class JwtGeneratorFixture : BaseFixture
    {
        private IJwtGenerator _jwtGenerator;

        private Mock<ISimpleIdentityServerConfigurator> _simpleIdentityServerConfigurator;

        #region GeneratedIdTokenPayloadForScopes

        [Test]
        public void When_Requesting_IdentityToken_JwsPayload_And_IndicateTheMaxAge_Then_TheJwsPayload_Contains_AuthenticationTime()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                MaxAge = 2
            };

            // ACT
            var result = _jwtGenerator.GenerateIdTokenPayloadForScopes(
                claimsPrincipal,
                authorizationParameter);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.IsTrue(result.ContainsKey(Core.Jwt.Constants.StandardClaimNames.AuthenticationTime));
            Assert.That(result[Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject].ToString(), Is.EqualTo(subject));
            Assert.IsNotEmpty(result[Core.Jwt.Constants.StandardClaimNames.AuthenticationTime].ToString());
        }

        [Test]
        public void When_Requesting_IdentityToken_JwsPayload_And_NumberOfAudiencesIsMoreThanOne_Then_Azp_Should_Be_Returned()
        {
            // ARRANGE
            InitializeMockObjects();
            const string issuerName = "IssuerName";
            var clientId = FakeDataSource.Instance().Clients.First().ClientId;
            const string subject = "habarthierry@hotmail.fr";
            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId
            };
            _simpleIdentityServerConfigurator.Setup(s => s.GetIssuerName()).Returns(issuerName);

            // ACT
            var result = _jwtGenerator.GenerateIdTokenPayloadForScopes(
                claimsPrincipal,
                authorizationParameter);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.IsTrue(result.Audiences.Count() > 1);
            Assert.IsTrue(result.Azp == clientId);
        }

        [Test]
        public void When_Requesting_IdentityToken_JwsPayload_And_ThereNoClient_Then_Azp_Should_Be_Returned()
        {
            // ARRANGE
            InitializeMockObjects();
            const string issuerName = "IssuerName";
            const string clientId = "clientId";
            FakeDataSource.Instance().Clients.Clear();
            const string subject = "habarthierry@hotmail.fr";
            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId
            };
            _simpleIdentityServerConfigurator.Setup(s => s.GetIssuerName()).Returns(issuerName);

            // ACT
            var result = _jwtGenerator.GenerateIdTokenPayloadForScopes(
                claimsPrincipal,
                authorizationParameter);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.IsTrue(result.Audiences.Count() == 1);
            Assert.IsTrue(result.Azp == clientId);
        }

        [Test]
        public void When_Requesting_IdentityToken_JwsPayload_With_No_Authorization_Request_Then_MandatoriesClaims_Are_Returned()
        {
            // ARRANGE
            InitializeMockObjects();
            const string issuerName = "IssuerName";
            const string subject = "habarthierry@hotmail.fr";
            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            _simpleIdentityServerConfigurator.Setup(s => s.GetIssuerName()).Returns(issuerName);

            // ACT
            var result = _jwtGenerator.GenerateIdTokenPayloadForScopes(
                claimsPrincipal,
                null);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey(Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.IsTrue(result.ContainsKey(Jwt.Constants.StandardClaimNames.Audiences));
            Assert.IsTrue(result.ContainsKey(Jwt.Constants.StandardClaimNames.ExpirationTime));
            Assert.IsTrue(result.ContainsKey(Jwt.Constants.StandardClaimNames.Iat));
            Assert.IsTrue(result.GetClaimValue(Jwt.Constants.StandardResourceOwnerClaimNames.Subject) == subject);
            Assert.IsTrue(result.Audiences.Contains(issuerName));
        }

        #endregion

        #region GenerateFilteredIdTokenPayload

        [Test]
        public void When_Requesting_IdentityToken_JwsPayload_And_Pass_AuthTime_As_ClaimEssential_Then_TheJwsPayload_Contains_AuthenticationTime()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Core.Jwt.Constants.StandardClaimNames.AuthenticationTime,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Core.Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                }
            };

            // ACT
            var result = _jwtGenerator.GenerateFilteredIdTokenPayload(
                claimsPrincipal,
                null,
                claimsParameter);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.IsTrue(result.ContainsKey(Core.Jwt.Constants.StandardClaimNames.AuthenticationTime));
            Assert.That(result[Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject].ToString(), Is.EqualTo(subject));
            Assert.That(long.Parse(result[Core.Jwt.Constants.StandardClaimNames.AuthenticationTime].ToString()), Is.EqualTo(currentDateTimeOffset));
        }

        [Test]
        public void When_Requesting_IdentityToken_JwsPayload_And_PassingANotValidClaimValue_Then_An_Exception_Is_Raised()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string notValidSubject = "habarthierry@hotmail.be";
            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };

            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Core.Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            Core.Constants.StandardClaimParameterValueNames.ValueName,
                            notValidSubject
                        }
                    }
                }
            };

            // ACT & ASSERTS
            var result = Assert.Throws<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredIdTokenPayload(
                claimsPrincipal,
                null,
                claimsParameter));

            Assert.That(result.Code, Is.EqualTo(ErrorCodes.InvalidGrant));
            Assert.That(result.Message, Is.EqualTo(string.Format(ErrorDescriptions.TheClaimIsNotValid, Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject)));
        }

        #endregion

        #region GenerateUserInfoPayloadForScope

        [Test]
        public void When_Requesting_UserInformation_JwsPayload_For_Scopes_Then_The_JwsPayload_Is_Correct()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string name = "Habart Thierry";
            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name, name),
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile"
            };

            // ACT
            var result = _jwtGenerator.GenerateUserInfoPayloadForScope(
                claimsPrincipal,
                authorizationParameter);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.IsTrue(result.ContainsKey(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name));
            Assert.That(result[Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject].ToString(), Is.EqualTo(subject));
            Assert.That(result[Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name].ToString(), Is.EqualTo(name));
        }

        #endregion

        #region GenerateFilteredUserInfoPayload

        [Test]
        public void When_Requesting_UserInformation_JwsPayload_For_CertainClaims_Then_The_JwsPayload_Is_Correct()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string name = "Habart Thierry";
            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name, name),
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Core.Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile"
            };

            // ACT
            var result = _jwtGenerator.GenerateFilteredUserInfoPayload(
                claimsParameter,
                claimsPrincipal,
                authorizationParameter);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.IsTrue(result.ContainsKey(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name));
            Assert.That(result[Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject].ToString(), Is.EqualTo(subject));
            Assert.That(result[Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name].ToString(), Is.EqualTo(name));
        }

        #endregion

        private void InitializeMockObjects()
        {
            _simpleIdentityServerConfigurator = new Mock<ISimpleIdentityServerConfigurator>();
            var clientRepository = FakeFactories.GetClientRepository();
            var clientValidator = new ClientValidator(clientRepository);
            var jsonWebKeyRepository = FakeFactories.GetJsonWebKeyRepository();
            var scopeRepository = FakeFactories.GetScopeRepository();
            var claimsMapping = new ClaimsMapping();
            var parameterParserHelper = new ParameterParserHelper(scopeRepository);
            var createJwsSignature = new CreateJwsSignature();
            var aesEncryptionHelper = new AesEncryptionHelper();
            var jweHelper = new JweHelper(aesEncryptionHelper);
            var jwsGenerator = new JwsGenerator(createJwsSignature);
            var jweGenerator = new JweGenerator(jweHelper);
            _jwtGenerator = new JwtGenerator(
                _simpleIdentityServerConfigurator.Object,
                clientRepository,
                clientValidator,
                jsonWebKeyRepository,
                scopeRepository,
                claimsMapping,
                parameterParserHelper,
                jwsGenerator,
                jweGenerator);
        }
    }
}
