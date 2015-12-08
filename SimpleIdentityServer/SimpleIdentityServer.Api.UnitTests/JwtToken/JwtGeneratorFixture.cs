using Moq;
using NUnit.Framework;
using SimpleIdentityServer.Api.UnitTests.Fake;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Encrypt.Encryption;
using SimpleIdentityServer.Core.Jwt.Mapping;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.Api.UnitTests.JwtToken
{
    [TestFixture]
    public class JwtGeneratorFixture : BaseFixture
    {
        private IJwtGenerator _jwtGenerator;

        private Mock<ISimpleIdentityServerConfigurator> _simpleIdentityServerConfigurator;
        
        [Test]
        public void When_Requesting_JwsPayload_And_Pass_AuthTime_As_ClaimEssential_Then_TheJwsPayload_Contains_AuthenticationTime()
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
        public void When_Requesting_JwsPayload_And_IndicateTheMaxAge_Then_TheJwsPayload_Contains_AuthenticationTime()
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
