using System.Globalization;
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
using SimpleIdentityServer.Core.Jwt.Serializer;

namespace SimpleIdentityServer.Core.UnitTests.JwtToken
{
    [TestFixture]
    public class JwtGeneratorFixture
    {
        private IJwtGenerator _jwtGenerator;

        private Mock<ISimpleIdentityServerConfigurator> _simpleIdentityServerConfigurator;

        #region GeneratedIdTokenPayloadForScopes

        [Test]
        public void When_Passing_Null_Parameters_To_GenerateIdTokenPayloadForScopes_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _jwtGenerator.GenerateIdTokenPayloadForScopes(null, null));
            Assert.Throws<ArgumentNullException>(() => _jwtGenerator.GenerateIdTokenPayloadForScopes(null, authorizationParameter));
        }

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
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
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
            Assert.IsTrue(result.ContainsKey(Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.IsTrue(result.ContainsKey(Jwt.Constants.StandardClaimNames.AuthenticationTime));
            Assert.That(result[Jwt.Constants.StandardResourceOwnerClaimNames.Subject].ToString(), Is.EqualTo(subject));
            Assert.IsNotEmpty(result[Jwt.Constants.StandardClaimNames.AuthenticationTime].ToString());
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
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
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
            Assert.IsTrue(result.ContainsKey(Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
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
            var authorizationParameter = new AuthorizationParameter();
            var claims = new List<Claim>
            {
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            _simpleIdentityServerConfigurator.Setup(s => s.GetIssuerName()).Returns(issuerName);

            // ACT
            var result = _jwtGenerator.GenerateIdTokenPayloadForScopes(
                claimsPrincipal,
                authorizationParameter);

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
        public void When_Passing_Null_Parameters_To_GenerateFilteredIdTokenPayload_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _jwtGenerator.GenerateFilteredIdTokenPayload(null, null, null));
            Assert.Throws<ArgumentNullException>(() => _jwtGenerator.GenerateFilteredIdTokenPayload(null, authorizationParameter, null));
        }

        [Test]
        public void When_Requesting_Identity_Token_And_Audiences_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string state = "state";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                State = state
            };
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Jwt.Constants.StandardClaimNames.Audiences,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            Constants.StandardClaimParameterValueNames.ValuesName,
                            new [] { "audience" }
                        }
                    }
                }
            };

            // ACT & ASSERT
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredIdTokenPayload(
                    claimsPrincipal,
                    authorizationParameter,
                    claimsParameter));
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidGrant);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardClaimNames.Audiences));
            Assert.IsTrue(exception.State == state);
        }

        [Test]
        public void When_Requesting_Identity_Token_And_Issuer_Claim_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string state = "state";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                State = state
            };
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Jwt.Constants.StandardClaimNames.Issuer,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            Constants.StandardClaimParameterValueNames.ValueName,
                            "issuer"
                        }
                    }
                }
            };
            _simpleIdentityServerConfigurator.Setup(s => s.GetIssuerName())
                .Returns("fake_issuer");

            // ACT & ASSERT
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredIdTokenPayload(
                    claimsPrincipal,
                    authorizationParameter,
                    claimsParameter));
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidGrant);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardClaimNames.Issuer));
            Assert.IsTrue(exception.State == state);
        }

        [Test]
        public void When_Requesting_Identity_Token_And_ExpirationTime_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string state = "state";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                State = state
            };
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Jwt.Constants.StandardClaimNames.ExpirationTime,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            Constants.StandardClaimParameterValueNames.ValueName,
                            12
                        }
                    }
                }
            };

            // ACT & ASSERT
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredIdTokenPayload(
                    claimsPrincipal,
                    authorizationParameter,
                    claimsParameter));
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidGrant);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardClaimNames.ExpirationTime));
            Assert.IsTrue(exception.State == state);
        }

        [Test]
        public void When_Requesting_IdentityToken_JwsPayload_And_PassingANotValidClaimValue_Then_An_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string notValidSubject = "habarthierry@hotmail.be";
            var claims = new List<Claim>
            {
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var authorizationParameter = new AuthorizationParameter();

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
                            Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            Constants.StandardClaimParameterValueNames.ValueName,
                            notValidSubject
                        }
                    }
                }
            };

            // ACT & ASSERTS
            var result = Assert.Throws<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredIdTokenPayload(
                claimsPrincipal,
                authorizationParameter,
                claimsParameter));

            Assert.That(result.Code, Is.EqualTo(ErrorCodes.InvalidGrant));
            Assert.That(result.Message, Is.EqualTo(string.Format(ErrorDescriptions.TheClaimIsNotValid, Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject)));
        }

        [Test]
        public void When_Requesting_IdentityToken_JwsPayload_And_Pass_AuthTime_As_ClaimEssential_Then_TheJwsPayload_Contains_AuthenticationTime()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string nonce = "nonce";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var authorizationParameter = new AuthorizationParameter
            {
                Nonce = nonce
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Jwt.Constants.StandardClaimNames.AuthenticationTime,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                },
                new ClaimParameter
                {
                    Name = Jwt.Constants.StandardClaimNames.Audiences,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Constants.StandardClaimParameterValueNames.ValuesName,
                            new [] { FakeDataSource.Instance().Clients.First().ClientId }
                        }
                    }
                },
                new ClaimParameter
                {
                    Name = Jwt.Constants.StandardClaimNames.Nonce,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Constants.StandardClaimParameterValueNames.ValueName,
                            nonce
                        }
                    }
                }
            };

            // ACT
            var result = _jwtGenerator.GenerateFilteredIdTokenPayload(
                claimsPrincipal,
                authorizationParameter,
                claimsParameter);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey(Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.IsTrue(result.ContainsKey(Jwt.Constants.StandardClaimNames.AuthenticationTime));
            Assert.IsTrue(result.ContainsKey(Jwt.Constants.StandardClaimNames.Nonce));
            Assert.That(result[Jwt.Constants.StandardResourceOwnerClaimNames.Subject].ToString(), Is.EqualTo(subject));
            Assert.That(long.Parse(result[Jwt.Constants.StandardClaimNames.AuthenticationTime].ToString()), Is.EqualTo(currentDateTimeOffset));
        }

        #endregion

        #region GenerateUserInfoPayloadForScope

        [Test]
        public void When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _jwtGenerator.GenerateUserInfoPayloadForScope(null, null));
            Assert.Throws<ArgumentNullException>(() => _jwtGenerator.GenerateUserInfoPayloadForScope(null, authorizationParameter));
        }

        [Test]
        public void When_Requesting_UserInformation_JwsPayload_For_Scopes_Then_The_JwsPayload_Is_Correct()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string name = "Habart Thierry";
            var claims = new List<Claim>
            {
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Name, name),
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
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
            Assert.IsTrue(result.ContainsKey(Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.IsTrue(result.ContainsKey(Jwt.Constants.StandardResourceOwnerClaimNames.Name));
            Assert.That(result[Jwt.Constants.StandardResourceOwnerClaimNames.Subject].ToString(), Is.EqualTo(subject));
            Assert.That(result[Jwt.Constants.StandardResourceOwnerClaimNames.Name].ToString(), Is.EqualTo(name));
        }
        
        #endregion

        #region GenerateFilteredUserInfoPayload

        [Test]
        public void When_Passing_Null_Parameters_To_GenerateFilteredUserInfoPayload_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            var authorizationParameter = new AuthorizationParameter();
            
            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _jwtGenerator.GenerateFilteredUserInfoPayload(null, null, null));
            Assert.Throws<ArgumentNullException>(() => _jwtGenerator.GenerateFilteredUserInfoPayload(null, null, authorizationParameter));
        }

        [Test]
        public void When_Requesting_UserInformation_But_The_Essential_Claim_Subject_Is_Empty_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "";
            const string state = "state";
            var claims = new List<Claim>
            {
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile",
                State = state
            };

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredUserInfoPayload(
                claimsParameter,
                claimsPrincipal,
                authorizationParameter));
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidGrant);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.IsTrue(exception.State == state);
        }

        [Test]
        public void When_Requesting_UserInformation_But_The_Subject_Claim_Value_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "invalid@loki.be";
            const string state = "state";
            var claims = new List<Claim>
            {
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Constants.StandardClaimParameterValueNames.ValueName,
                            "habarthierry@lokie.be"
                        }
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile",
                State = state
            };

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredUserInfoPayload(
                claimsParameter,
                claimsPrincipal,
                authorizationParameter));
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidGrant);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.IsTrue(exception.State == state);
        }
        
        [Test]
        public void When_Requesting_UserInformation_But_The_Essential_Claim_Name_Is_Empty_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string state = "state";
            var claims = new List<Claim>
            {
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Jwt.Constants.StandardResourceOwnerClaimNames.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile",
                State = state
            };

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredUserInfoPayload(
                claimsParameter,
                claimsPrincipal,
                authorizationParameter));
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidGrant);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardResourceOwnerClaimNames.Name));
            Assert.IsTrue(exception.State == state);
        }

        [Test]
        public void When_Requesting_UserInformation_But_The_Name_Claim_Value_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@lokie.be";
            const string state = "state";
            var claims = new List<Claim>
            {
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject),
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Name, "invalid_name")
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Constants.StandardClaimParameterValueNames.ValueName,
                            subject
                        }
                    }
                },
                new ClaimParameter
                {
                    Name = Jwt.Constants.StandardResourceOwnerClaimNames.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Constants.StandardClaimParameterValueNames.ValueName,
                            "name"
                        }
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile",
                State = state
            };

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _jwtGenerator.GenerateFilteredUserInfoPayload(
                claimsParameter,
                claimsPrincipal,
                authorizationParameter));
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Code == ErrorCodes.InvalidGrant);
            Assert.IsTrue(exception.Message == string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardResourceOwnerClaimNames.Name));
            Assert.IsTrue(exception.State == state);
        }

        [Test]
        public void When_Requesting_UserInformation_For_Some_Valid_Claims_Then_The_JwsPayload_Is_Correct()
        {
            // ARRANGE
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string name = "Habart Thierry";
            var claims = new List<Claim>
            {
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Name, name),
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = Jwt.Constants.StandardResourceOwnerClaimNames.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                },
                new ClaimParameter
                {
                    Name = Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            Constants.StandardClaimParameterValueNames.ValueName,
                            subject
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
            var createJwsSignature = new CreateJwsSignature(new CngKeySerializer());
            var aesEncryptionHelper = new AesEncryptionHelper();
            var jweHelper = new JweHelper(aesEncryptionHelper);
            var jwsGenerator = new JwsGenerator(createJwsSignature);
            var jweGenerator = new JweGenerator(jweHelper);

            FakeDataSource.Instance().Scopes = FakeOpenIdAssets.GetScopes();
            FakeDataSource.Instance().Clients = FakeOpenIdAssets.GetClients();

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
