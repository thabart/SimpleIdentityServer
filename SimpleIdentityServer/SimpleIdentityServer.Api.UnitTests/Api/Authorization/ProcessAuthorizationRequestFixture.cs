using System;
using System.Linq;
using NUnit.Framework;
using SimpleIdentityServer.Api.UnitTests.Fake;
using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.DataAccess.Fake;
using SimpleIdentityServer.DataAccess.Fake.Models;
using System.Security.Claims;
using System.Collections.Generic;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Encrypt.Encryption;
using SimpleIdentityServer.Core.Jwt.Signature;
using Moq;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Mapping;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Api.UnitTests.Api.Authorization
{
    [TestFixture]
    public sealed class ProcessAuthorizationRequestFixture : BaseFixture
    {
        private ProcessAuthorizationRequest _processAuthorizationRequest;

        private Mock<ISimpleIdentityServerConfigurator> _simpleIdentityServerConfigurator;

        private JwtGenerator _jwtGenerator;

        private IJsonWebKeyRepository _jsonWebKeyRepository;

        #region TEST FAILURES

        [Test]
        public void When_Passing_NullAuthorization_To_Function_Then_ArgumentNullException_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _processAuthorizationRequest.Process(null, null, string.Empty));
        }

        [Test]
        public void When_Passing_NoEncryptedRequest_To_Function_Then_ArgumentNullException_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _processAuthorizationRequest.Process(authorizationParameter, null, string.Empty));
        }

        [Test]
        public void When_Passing_NoneExisting_ClientId_To_AuthorizationParameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "fake_client";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = "fake_client",
                Prompt = "login",
                State = state
            };

            // ACT & ASSERTS
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, null, code));
            Assert.That(exception.Code, Is.EqualTo(ErrorCodes.InvalidClient));
            Assert.That(exception.Message, Is.EqualTo(string.Format(ErrorDescriptions.ClientIsNotValid, clientId)));
            Assert.That(exception.State, Is.EqualTo(state));
        }

        [Test]
        public void When_Passing_NotValidRedirectUrl_To_AuthorizationParameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string redirectUrl = "not valid redirect url";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "login",
                State = state,
                RedirectUrl = redirectUrl
            };

            // ACT & ASSERTS
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, null, code));
            Assert.That(exception.Code, Is.EqualTo(ErrorCodes.InvalidRequestCode));
            Assert.That(exception.Message, Is.EqualTo(string.Format(ErrorDescriptions.RedirectUrlIsNotValid, redirectUrl)));
            Assert.That(exception.State, Is.EqualTo(state));
        }

        [Test]
        public void When_Passing_AuthorizationParameterWithoutOpenIdScope_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "login",
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "email"
            };

            // ACT & ASSERTS
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, null, code));
            Assert.That(exception.Code, Is.EqualTo(ErrorCodes.InvalidScope));
            Assert.That(exception.Message, Is.EqualTo(string.Format(ErrorDescriptions.TheScopesNeedToBeSpecified, Core.Constants.StandardScopes.OpenId.Name)));
            Assert.That(exception.State, Is.EqualTo(state));
        }

        [Test]
        public void When_Passing_AuthorizationRequestWithMissingResponseType_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "login",
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid"
            };

            // ACT & ASSERTS
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, null, code));
            Assert.That(exception.Code, Is.EqualTo(ErrorCodes.InvalidRequestCode));
            Assert.That(exception.Message, Is.EqualTo(string.Format(ErrorDescriptions.MissingParameter
                , Core.Constants.StandardAuthorizationRequestParameterNames.ResponseTypeName)));
            Assert.That(exception.State, Is.EqualTo(state));
        }

        [Test]
        public void When_Passing_AuthorizationRequestWithNotSupportedResponseType_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "login",
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code"
            };

            // ACT
            var client = FakeDataSource.Instance().Clients.FirstOrDefault(c => c.ClientId == clientId);
            Assert.IsNotNull(client);
            client.ResponseTypes.Remove(ResponseType.code);

            // ACT & ASSERTS
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, null, code));
            Assert.That(exception.Code, Is.EqualTo(ErrorCodes.InvalidRequestCode));
            Assert.That(exception.Message, Is.EqualTo(string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType
                , clientId, "code")));
            Assert.That(exception.State, Is.EqualTo(state));
        }

        [Test]
        public void When_TryingToByPassLoginAndConsentScreen_But_UserIsNotAuthenticated_Then_Exception_Is_Thrown()
        {            
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "none",
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code"
            };

            // ACT & ASSERTS
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, null, code));
            Assert.That(exception.Code, Is.EqualTo(ErrorCodes.LoginRequiredCode));
            Assert.That(exception.Message, Is.EqualTo(ErrorDescriptions.TheUserNeedsToBeAuthenticated));
            Assert.That(exception.State, Is.EqualTo(state));
        }

        [Test]
        public void When_TryingToByPassLoginAndConsentScreen_But_TheUserDidntGiveHisConsent_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "none",
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code"
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));

            // ACT & ASSERTS
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal, code));
            Assert.That(exception.Code, Is.EqualTo(ErrorCodes.InteractionRequiredCode));
            Assert.That(exception.Message, Is.EqualTo(ErrorDescriptions.TheUserNeedsToGiveHisConsent));
            Assert.That(exception.State, Is.EqualTo(state));
        }

        [Test]
        public void When_Passing_A_NotValid_IdentityTokenHint_Parameter_Then_An_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string subject = "habarthierry@hotmail.fr";
            const string redirectUrl = "http://localhost";
            FakeDataSource.Instance().Consents.Add(new Consent
            {
                ResourceOwner = new ResourceOwner
                {
                    Id = subject
                },
                GrantedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                },
                Client = FakeDataSource.Instance().Clients.First()
            });
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code",
                Prompt = "none",
                IdTokenHint = "invalid identity token hint"
            };

            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);

            // ACT
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal, code));
            Assert.That(exception.Code, Is.EqualTo(ErrorCodes.InvalidRequestCode));
            Assert.That(exception.Message, Is.EqualTo(ErrorDescriptions.TheSignatureOfIdTokenHintParameterCannotBeChecked));
        }

        [Test]
        public void When_Passing_An_IdentityToken_Valid_ForWrongAudience_Then_An_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string subject = "habarthierry@hotmail.fr";
            const string redirectUrl = "http://localhost";
            FakeDataSource.Instance().Consents.Add(new Consent
            {
                ResourceOwner = new ResourceOwner
                {
                    Id = subject
                },
                GrantedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                },
                Client = FakeDataSource.Instance().Clients.First()
            });
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code",
                Prompt = "none",
            };

            var subjectClaim = new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject);
            var claims = new List<Claim>
            {
                subjectClaim
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var jwtPayload = new JwsPayload
            {
                {
                    subjectClaim.Type, subjectClaim.Value
                }
            };

            authorizationParameter.IdTokenHint = _jwtGenerator.Sign(jwtPayload, clientId);

            // ACT
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal, code));
            Assert.That(exception.Code, Is.EqualTo(ErrorCodes.InvalidRequestCode));
            Assert.That(exception.Message, Is.EqualTo(ErrorDescriptions.TheIdentityTokenDoesntContainSimpleIdentityServerAsAudience));
        }


        [Test]
        public void When_Passing_An_IdentityToken_Different_From_The_Current_Authenticated_User__Then_An_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string subject = "habarthierry@hotmail.fr";
            const string issuerName = "audience";
            const string redirectUrl = "http://localhost";
            FakeDataSource.Instance().Consents.Add(new Consent
            {
                ResourceOwner = new ResourceOwner
                {
                    Id = subject
                },
                GrantedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                },
                Client = FakeDataSource.Instance().Clients.First()
            });
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code",
                Prompt = "none",
            };

            var subjectClaim = new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject);
            var claims = new List<Claim>
            {
                subjectClaim
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var jwtPayload = new JwsPayload
            {
                {
                    subjectClaim.Type, "wrong subjet"
                },
                {
                    Constants.StandardClaimNames.Audiences, new string[] {  issuerName }
                }
            };
            _simpleIdentityServerConfigurator.Setup(s => s.GetIssuerName()).Returns(issuerName);

            authorizationParameter.IdTokenHint = _jwtGenerator.Sign(jwtPayload, clientId);

            // ACT
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal, code));
            Assert.That(exception.Code, Is.EqualTo(ErrorCodes.InvalidRequestCode));
            Assert.That(exception.Message, Is.EqualTo(ErrorDescriptions.TheCurrentAuthenticatedUserDoesntMatchWithTheIdentityToken));
        }

        #endregion

        #region TEST VALID SCENARIOS

        [Test]
        public void When_TryingToRequestAuthorization_But_TheUserConnectionValidityPeriodIsNotValid_Then_Redirect_To_The_Authentication_Screen()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            const long maxAge = 300;
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            currentDateTimeOffset -= maxAge + 100;
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                Prompt = "none",
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code",
                MaxAge = 300
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString())
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);

            // ACT
            var result = _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal, code);

            // ASSERTS
            Assert.IsNotNull(result);
            Assert.That(result.RedirectInstruction.Action, Is.EqualTo(Core.Results.IdentityServerEndPoints.AuthenticateIndex));
            Assert.That(result.RedirectInstruction.Parameters.Count(), Is.EqualTo(1));
            Assert.That(result.RedirectInstruction.Parameters.First().Name, Is.EqualTo(Core.Constants.StandardAuthorizationResponseNames.AuthorizationCodeName));
            Assert.That(result.RedirectInstruction.Parameters.First().Value, Is.EqualTo(code));
        }

        [Test]
        public void When_TryingToRequestAuthorization_But_TheUserIsNotAuthenticated_Then_Redirect_To_The_Authentication_Screen()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code",
            };

            // ACT
            var result = _processAuthorizationRequest.Process(authorizationParameter, null, code);

            // ASSERTS
            Assert.IsNotNull(result);
            Assert.That(result.RedirectInstruction.Action, Is.EqualTo(Core.Results.IdentityServerEndPoints.AuthenticateIndex));
            Assert.That(result.RedirectInstruction.Parameters.Count(), Is.EqualTo(1));
            Assert.That(result.RedirectInstruction.Parameters.First().Name, Is.EqualTo(Core.Constants.StandardAuthorizationResponseNames.AuthorizationCodeName));
            Assert.That(result.RedirectInstruction.Parameters.First().Value, Is.EqualTo(code));
        }

        [Test]
        public void When_TryingToRequestAuthorization_And_TheUserIsAuthenticated_But_He_Didnt_Give_His_Consent_Then_Redirect_To_Consent_Screen()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code",
            };
            
            var claimIdentity = new ClaimsIdentity("fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);

            // ACT
            var result = _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal, code);

            // ASSERTS
            Assert.IsNotNull(result);
            Assert.That(result.RedirectInstruction.Action, Is.EqualTo(Core.Results.IdentityServerEndPoints.ConsentIndex));
            Assert.That(result.RedirectInstruction.Parameters.Count(), Is.EqualTo(1));
            Assert.That(result.RedirectInstruction.Parameters.First().Name, Is.EqualTo(Core.Constants.StandardAuthorizationResponseNames.AuthorizationCodeName));
            Assert.That(result.RedirectInstruction.Parameters.First().Value, Is.EqualTo(code));
        }

        [Test]
        public void When_TryingToRequestAuthorization_And_ExplicitySpecify_PromptConsent_But_The_User_IsNotAuthenticated_Then_Redirect_To_Consent_Screen()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code",
                Prompt = "consent"
            };

            // ACT
            var result = _processAuthorizationRequest.Process(authorizationParameter, null, code);

            // ASSERTS
            Assert.IsNotNull(result);
            Assert.That(result.RedirectInstruction.Action, Is.EqualTo(Core.Results.IdentityServerEndPoints.AuthenticateIndex));
            Assert.That(result.RedirectInstruction.Parameters.Count(), Is.EqualTo(1));
            Assert.That(result.RedirectInstruction.Parameters.First().Name, Is.EqualTo(Core.Constants.StandardAuthorizationResponseNames.AuthorizationCodeName));
            Assert.That(result.RedirectInstruction.Parameters.First().Value, Is.EqualTo(code));
        }

        [Test]
        public void When_TryingToRequestAuthorization_And_TheUserIsAuthenticated_And_He_Already_Gave_HisConsent_Then_The_AuthorizationCode_Is_Passed_To_The_Callback()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string code = "code";
            const string clientId = "MyBlog";
            const string subject = "habarthierry@hotmail.fr";
            const string redirectUrl = "http://localhost";
            FakeDataSource.Instance().Consents.Add(new Consent
            {
                ResourceOwner = new ResourceOwner
                {
                    Id = subject
                },
                GrantedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                },
                Client = FakeDataSource.Instance().Clients.First()
            });
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code",
                Prompt = "none"
            };

            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);

            // ACT
            var result = _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal, code);
            
            // ASSERTS
            Assert.IsNotNull(result);
            Assert.That(result.Type, Is.EqualTo(TypeActionResult.RedirectToCallBackUrl));
            Assert.That(result.RedirectInstruction.Parameters.Count(), Is.EqualTo(0));
        }
        
        #endregion

        private void InitializeMockingObjects()
        {
            _simpleIdentityServerConfigurator = new Mock<ISimpleIdentityServerConfigurator>();
            var scopeRepository = FakeFactories.GetScopeRepository();
            var clientRepository = FakeFactories.GetClientRepository();
            var consentRepository = FakeFactories.GetConsentRepository();
            var jsonWebKeyRepository = FakeFactories.GetJsonWebKeyRepository();
            var parameterParserHelper = new ParameterParserHelper(scopeRepository);
            var clientValidator = new ClientValidator(clientRepository);
            var scopeValidator = new ScopeValidator(parameterParserHelper);
            var actionResultFactory = new ActionResultFactory();
            var consentHelper = new ConsentHelper(consentRepository, parameterParserHelper);
            var aesEncryptionHelper = new AesEncryptionHelper();
            var jweHelper = new JweHelper(aesEncryptionHelper);
            var jweParser = new JweParser(jweHelper);
            var createJwsSignature = new CreateJwsSignature();
            var jwsParser = new JwsParser(createJwsSignature);
            var jwtParser = new JwtParser(jweParser, jwsParser, jsonWebKeyRepository);
            var claimsMapping = new ClaimsMapping();
            var jwsGenerator = new JwsGenerator(createJwsSignature);
            var jweGenerator = new JweGenerator(jweHelper);

            _jsonWebKeyRepository = jsonWebKeyRepository;
            _processAuthorizationRequest = new ProcessAuthorizationRequest(
                parameterParserHelper,
                clientValidator,
                scopeValidator,
                actionResultFactory,
                consentHelper,
                jwtParser,
                _simpleIdentityServerConfigurator.Object);
            _jwtGenerator = new JwtGenerator(_simpleIdentityServerConfigurator.Object,
                clientRepository,
                clientValidator,
                jsonWebKeyRepository,
                scopeRepository,
                claimsMapping,
                parameterParserHelper,
                jwsGenerator,
                jweGenerator);
        }

        [OneTimeTearDown]
        public void CleanFakeData()
        {
            FakeDataSource.Instance().Init();
        }
    }
}
