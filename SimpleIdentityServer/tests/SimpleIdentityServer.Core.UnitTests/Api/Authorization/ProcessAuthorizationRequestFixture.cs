﻿using Moq;
using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Encrypt.Encryption;
using SimpleIdentityServer.Core.Jwt.Mapping;
using SimpleIdentityServer.Core.Jwt.Serializer;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.OAuth.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Authorization
{
    using Core.Common.Models;

    public sealed class ProcessAuthorizationRequestFixture
    {
        private ProcessAuthorizationRequest _processAuthorizationRequest;
        private Mock<IConfigurationService> _simpleIdentityServerConfiguratorStub;
        private Mock<IOAuthEventSource> _oauthEventSource;
        private JwtGenerator _jwtGenerator;

        #region TEST FAILURES

        [Fact]
        public async Task When_Passing_NullAuthorization_To_Function_Then_ArgumentNullException_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _processAuthorizationRequest.ProcessAsync(null, null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _processAuthorizationRequest.ProcessAsync(new AuthorizationParameter(), null, null)).ConfigureAwait(false);
        }
        
        [Fact]
        public async Task When_Passing_NotValidRedirectUrl_To_AuthorizationParameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
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
            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(
                () => _processAuthorizationRequest.ProcessAsync(authorizationParameter, null, new Client())).ConfigureAwait(false);
            Assert.True(exception.Code.Equals(ErrorCodes.InvalidRequestCode));
            Assert.True(exception.Message.Equals(string.Format(ErrorDescriptions.RedirectUrlIsNotValid, redirectUrl)));
            Assert.True(exception.State.Equals(state));
        }

        /*
        [Fact]
        public void When_Passing_AuthorizationParameterWithoutOpenIdScope_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
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
                    () => _processAuthorizationRequest.Process(authorizationParameter, null));
            Assert.True(exception.Code.Equals(ErrorCodes.InvalidScope));
            Assert.True(exception.Message.Equals(string.Format(ErrorDescriptions.TheScopesNeedToBeSpecified, Core.Constants.StandardScopes.OpenId.Name)));
            Assert.True(exception.State.Equals(state));
        }

        [Fact]
        public void When_Passing_AuthorizationRequestWithMissingResponseType_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
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
                    () => _processAuthorizationRequest.Process(authorizationParameter, null));
            Assert.True(exception.Code.Equals(ErrorCodes.InvalidRequestCode));
            Assert.True(exception.Message.Equals(string.Format(ErrorDescriptions.MissingParameter
                , Core.Constants.StandardAuthorizationRequestParameterNames.ResponseTypeName)));
            Assert.True(exception.State.Equals(state));
        }

        [Fact]
        public void When_Passing_AuthorizationRequestWithNotSupportedResponseType_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
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
            var client = FakeFactories.FakeDataSource.Clients.FirstOrDefault(c => c.ClientId == clientId);
            Assert.NotNull(client);
            client.ResponseTypes.Remove(ResponseType.code);

            // ACT & ASSERTS
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, null));
            Assert.True(exception.Code.Equals(ErrorCodes.InvalidRequestCode));
            Assert.True(exception.Message.Equals(string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType
                , clientId, "code")));
            Assert.True(exception.State.Equals(state));
        }

        [Fact]
        public void When_TryingToByPassLoginAndConsentScreen_But_UserIsNotAuthenticated_Then_Exception_Is_Thrown()
        {            
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
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
                    () => _processAuthorizationRequest.Process(authorizationParameter, null));
            Assert.True(exception.Code.Equals(ErrorCodes.LoginRequiredCode));
            Assert.True(exception.Message.Equals(ErrorDescriptions.TheUserNeedsToBeAuthenticated));
            Assert.True(exception.State.Equals(state));
        }

        [Fact]
        public void When_TryingToByPassLoginAndConsentScreen_But_TheUserDidntGiveHisConsent_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
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
                    () => _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal));
            Assert.True(exception.Code.Equals(ErrorCodes.InteractionRequiredCode));
            Assert.True(exception.Message.Equals(ErrorDescriptions.TheUserNeedsToGiveHisConsent));
            Assert.True(exception.State.Equals(state));
        }

        [Fact]
        public void When_Passing_A_NotValid_IdentityTokenHint_Parameter_Then_An_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string subject = "habarthierry@hotmail.fr";
            const string redirectUrl = "http://localhost";
            FakeFactories.FakeDataSource.Consents.Add(new Consent
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
                Client = FakeFactories.FakeDataSource.Clients.First()
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
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);

            // ACT
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal));
            Assert.True(exception.Code.Equals(ErrorCodes.InvalidRequestCode));
            Assert.True(exception.Message.Equals(ErrorDescriptions.TheIdTokenHintParameterIsNotAValidToken));
        }

        [Fact]
        public void When_Passing_An_IdentityToken_Valid_ForWrongAudience_Then_An_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string subject = "habarthierry@hotmail.fr";
            const string redirectUrl = "http://localhost";
            FakeFactories.FakeDataSource.Consents.Add(new Consent
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
                Client = FakeFactories.FakeDataSource.Clients.First()
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

            authorizationParameter.IdTokenHint = _jwtGenerator.Sign(jwtPayload, JwsAlg.RS256);

            // ACT
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal));
            Assert.True(exception.Code.Equals(ErrorCodes.InvalidRequestCode));
            Assert.True(exception.Message.Equals(ErrorDescriptions.TheIdentityTokenDoesntContainSimpleIdentityServerAsAudience));
        }
        
        [Fact]
        public void When_Passing_An_IdentityToken_Different_From_The_Current_Authenticated_User_Then_An_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string subject = "habarthierry@hotmail.fr";
            const string issuerName = "audience";
            const string redirectUrl = "http://localhost";
            FakeFactories.FakeDataSource.Consents.Add(new Consent
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
                Client = FakeFactories.FakeDataSource.Clients.First()
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
                    Jwt.Constants.StandardClaimNames.Audiences, new [] {  issuerName }
                }
            };
            _simpleIdentityServerConfiguratorStub.Setup(s => s.GetIssuerName()).Returns(issuerName);
            authorizationParameter.IdTokenHint = _jwtGenerator.Sign(jwtPayload, JwsAlg.RS256);

            // ACT
            var exception = Assert.Throws<IdentityServerExceptionWithState>(() => _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal));
            Assert.True(exception.Code.Equals(ErrorCodes.InvalidRequestCode));
            Assert.True(exception.Message.Equals(ErrorDescriptions.TheCurrentAuthenticatedUserDoesntMatchWithTheIdentityToken));
        }

        [Fact]
        public void When_Passing_Not_Supported_Prompts_Parameter_Then_An_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "select_account",
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code"
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));

            // ACT & ASSERTS
            var exception =
                Assert.Throws<IdentityServerExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal));
            Assert.True(exception.Code.Equals(ErrorCodes.InvalidRequestCode));
            Assert.True(exception.Message.Equals(string.Format(ErrorDescriptions.ThePromptParameterIsNotSupported, "select_account")));
            Assert.True(exception.State.Equals(state));
        }
        */
        #endregion
        /*
        #region TEST VALID SCENARIOS

        [Fact]
        public void When_TryingToRequestAuthorization_But_TheUserConnectionValidityPeriodIsNotValid_Then_Redirect_To_The_Authentication_Screen()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
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
            var result = _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.RedirectInstruction.Action.Equals(IdentityServerEndPoints.AuthenticateIndex));
        }

        [Fact]
        public void When_TryingToRequestAuthorization_But_TheUserIsNotAuthenticated_Then_Redirect_To_The_Authentication_Screen()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
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
            var result = _processAuthorizationRequest.Process(authorizationParameter, null);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.RedirectInstruction.Action.Equals(Core.Results.IdentityServerEndPoints.AuthenticateIndex));
        }

        [Fact]
        public void When_TryingToRequestAuthorization_And_TheUserIsAuthenticated_But_He_Didnt_Give_His_Consent_Then_Redirect_To_Consent_Screen()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
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
            var result = _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.RedirectInstruction.Action.Equals(Core.Results.IdentityServerEndPoints.ConsentIndex));
        }

        [Fact]
        public void When_TryingToRequestAuthorization_And_ExplicitySpecify_PromptConsent_But_The_User_IsNotAuthenticated_Then_Redirect_To_Consent_Screen()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
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
            var result = _processAuthorizationRequest.Process(authorizationParameter, null);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.RedirectInstruction.Action.Equals(Core.Results.IdentityServerEndPoints.AuthenticateIndex));
        }

        [Fact]
        public void When_TryingToRequestAuthorization_And_TheUserIsAuthenticated_And_He_Already_Gave_HisConsent_Then_The_AuthorizationCode_Is_Passed_To_The_Callback()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string subject = "habarthierry@hotmail.fr";
            const string redirectUrl = "http://localhost";
            FakeFactories.FakeDataSource.Consents.Add(new Consent
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
                Client = FakeFactories.FakeDataSource.Clients.First()
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
            var result = _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal);
            
            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Type.Equals(TypeActionResult.RedirectToCallBackUrl));
            Assert.True(result.RedirectInstruction.Parameters.Count().Equals(0));
        }
        
        #endregion

        #region TEST THE LOGIN

        [Fact]
        public void When_Executing_Correct_Authorization_Request_Then_Events_Are_Logged()
        {
            // ARRANGE
            InitializeMockingObjects();
            const string state = "state";
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

            var jsonAuthorizationParameter = authorizationParameter.SerializeWithJavascript();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString())
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);

            // ACT
            var result = _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.RedirectInstruction.Action.Equals(IdentityServerEndPoints.AuthenticateIndex));
            _simpleIdentityServerEventSource.Verify(s => s.StartProcessingAuthorizationRequest(jsonAuthorizationParameter));
            _simpleIdentityServerEventSource.Verify(s => s.EndProcessingAuthorizationRequest(jsonAuthorizationParameter, "RedirectToAction", "AuthenticateIndex"));
        }

        #endregion
        */
        private void InitializeMockingObjects()
        {
            var clientValidator = new ClientValidator();
            _simpleIdentityServerConfiguratorStub = new Mock<IConfigurationService>();
            _oauthEventSource = new Mock<IOAuthEventSource>();
            var scopeRepository = new Mock<IScopeRepository>();
            var clientRepository = new Mock<IClientRepository>();
            var consentRepository = new Mock<IConsentRepository>();
            var jsonWebKeyRepository = new Mock<IJsonWebKeyRepository>();
            var parameterParserHelper = new ParameterParserHelper(scopeRepository.Object);
            var scopeValidator = new ScopeValidator(parameterParserHelper);
            var actionResultFactory = new ActionResultFactory();
            var consentHelper = new ConsentHelper(consentRepository.Object, parameterParserHelper);
            var aesEncryptionHelper = new AesEncryptionHelper();
            var jweHelper = new JweHelper(aesEncryptionHelper);
            var jweParser = new JweParser(jweHelper);
            var createJwsSignature = new CreateJwsSignature(new CngKeySerializer());
            var jwsParser = new JwsParser(createJwsSignature);
            var jsonWebKeyConverter = new JsonWebKeyConverter();
            var httpClientFactory = new HttpClientFactory();
            var jwtParser = new JwtParser(
                jweParser, 
                jwsParser, 
                httpClientFactory,
                clientRepository.Object,
                jsonWebKeyConverter,
                jsonWebKeyRepository.Object);
            var claimsMapping = new ClaimsMapping();
            var jwsGenerator = new JwsGenerator(createJwsSignature);
            var jweGenerator = new JweGenerator(jweHelper);

            _processAuthorizationRequest = new ProcessAuthorizationRequest(
                parameterParserHelper,
                clientValidator,
                scopeValidator,
                actionResultFactory,
                consentHelper,
                jwtParser,
                _simpleIdentityServerConfiguratorStub.Object,
                _oauthEventSource.Object);
            _jwtGenerator = new JwtGenerator(_simpleIdentityServerConfiguratorStub.Object,
                clientRepository.Object,
                clientValidator,
                jsonWebKeyRepository.Object,
                scopeRepository.Object,
                claimsMapping,
                parameterParserHelper,
                jwsGenerator,
                jweGenerator);
        }
    }
}