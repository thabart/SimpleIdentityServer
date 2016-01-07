using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Moq;
using NUnit.Framework;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Core.UnitTests.Common
{
    [TestFixture]
    public sealed  class GenerateAuthorizationResponseFixture
    {
        private Mock<IAuthorizationCodeRepository> _authorizationCodeRepositoryFake;

        private Mock<IParameterParserHelper> _parameterParserHelperFake;

        private Mock<IJwtGenerator> _jwtGeneratorFake;

        private Mock<IGrantedTokenGeneratorHelper> _grantedTokenGeneratorHelperFake;

        private Mock<IGrantedTokenRepository> _grantedTokenRepositoryFake;

        private Mock<IConsentHelper> _consentHelperFake;

        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSource;

        private Mock<IAuthorizationFlowHelper> _authorizationFlowHelperFake; 

        private IGenerateAuthorizationResponse _generateAuthorizationResponse;

        [Test]
        public void When_Passing_No_Authorization_Request_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _generateAuthorizationResponse.Execute(null, null, null));
        }

        [Test]
        public void When_Passing_No_ActionResult_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            
            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(
                () => _generateAuthorizationResponse.Execute(null, new AuthorizationParameter(), null));
        }

        [Test]
        public void When_There_Is_No_Logged_User_Then_Exception_Is_Throw()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(
                () => _generateAuthorizationResponse.Execute(new ActionResult(), new AuthorizationParameter(), null));
        }

        [Test]
        public void When_Generating_AuthorizationResponse_With_IdToken_Then_IdToken_Is_Added_To_The_Parameters()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            const string idToken = "idToken";
            var authorizationParameter = new AuthorizationParameter();
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var jwsPayload = new JwsPayload();
            _parameterParserHelperFake.Setup(p => p.ParseResponseType(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.id_token  
                });
            _jwtGeneratorFake.Setup(
                j => j.GenerateIdTokenPayloadForScopes(It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthorizationParameter>()))
                .Returns(jwsPayload);
            _jwtGeneratorFake.Setup(
                j => j.GenerateUserInfoPayloadForScope(It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthorizationParameter>()))
                .Returns(jwsPayload);
            _jwtGeneratorFake.Setup(j => j.Encrypt(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(idToken);

            // ACT
            _generateAuthorizationResponse.Execute(actionResult, authorizationParameter, claimsPrincipal);

            // ASSERT
            Assert.IsTrue(actionResult.RedirectInstruction.Parameters.Any(p => p.Name == Core.Constants.StandardAuthorizationResponseNames.IdTokenName));
            Assert.IsTrue(actionResult.RedirectInstruction.Parameters.Any(p => p.Value == idToken));
        }

        [Test]
        public void When_Generating_AuthorizationResponse_With_AccessToken_And_ThereIs_No_Granted_Token_Then_Token_Is_Generated_And_Added_To_The_Parameters()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "openid";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = scope
            };
            var grantedToken = new GrantedToken
            {
                AccessToken = Guid.NewGuid().ToString()
            };
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var jwsPayload = new JwsPayload();
            _parameterParserHelperFake.Setup(p => p.ParseResponseType(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.token  
                });
            _jwtGeneratorFake.Setup(
                j => j.GenerateIdTokenPayloadForScopes(It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthorizationParameter>()))
                .Returns(jwsPayload);
            _jwtGeneratorFake.Setup(
                j => j.GenerateUserInfoPayloadForScope(It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthorizationParameter>()))
                .Returns(jwsPayload);
            _jwtGeneratorFake.Setup(j => j.Encrypt(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(idToken);
            _parameterParserHelperFake.Setup(p => p.ParseScopeParameters(It.IsAny<string>()))
                .Returns(() => new List<string> { scope });
            _grantedTokenRepositoryFake.Setup(r => r.GetToken(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns(() => null);
            _grantedTokenGeneratorHelperFake.Setup(r => r.GenerateToken(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns(grantedToken);

            // ACT
            _generateAuthorizationResponse.Execute(actionResult, authorizationParameter, claimsPrincipal);

            // ASSERTS
            Assert.IsTrue(actionResult.RedirectInstruction.Parameters.Any(p => p.Name == Core.Constants.StandardAuthorizationResponseNames.AccessTokenName));
            Assert.IsTrue(actionResult.RedirectInstruction.Parameters.Any(p => p.Value == grantedToken.AccessToken));
            _grantedTokenRepositoryFake.Verify(g => g.Insert(grantedToken));
            _simpleIdentityServerEventSource.Verify(e => e.GrantAccessToClient(clientId, grantedToken.AccessToken, scope));
        }

        [Test]
        public void When_Generating_AuthorizationResponse_With_AccessToken_And_ThereIs_A_GrantedToken_Then_Token_Is_Added_To_The_Parameters()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "openid";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = scope
            };
            var grantedToken = new GrantedToken
            {
                AccessToken = Guid.NewGuid().ToString()
            };
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var jwsPayload = new JwsPayload();
            _parameterParserHelperFake.Setup(p => p.ParseResponseType(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.token  
                });
            _jwtGeneratorFake.Setup(
                j => j.GenerateIdTokenPayloadForScopes(It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthorizationParameter>()))
                .Returns(jwsPayload);
            _jwtGeneratorFake.Setup(
                j => j.GenerateUserInfoPayloadForScope(It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthorizationParameter>()))
                .Returns(jwsPayload);
            _jwtGeneratorFake.Setup(j => j.Encrypt(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(idToken);
            _parameterParserHelperFake.Setup(p => p.ParseScopeParameters(It.IsAny<string>()))
                .Returns(() => new List<string> { scope });
            _grantedTokenRepositoryFake.Setup(r => r.GetToken(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<JwsPayload>(),
                It.IsAny<JwsPayload>()))
                .Returns(() => grantedToken);

            // ACT
            _generateAuthorizationResponse.Execute(actionResult, authorizationParameter, claimsPrincipal);

            // ASSERTS
            Assert.IsTrue(actionResult.RedirectInstruction.Parameters.Any(p => p.Name == Core.Constants.StandardAuthorizationResponseNames.AccessTokenName));
            Assert.IsTrue(actionResult.RedirectInstruction.Parameters.Any(p => p.Value == grantedToken.AccessToken));
        }

        [Test]
        public void When_Generating_AuthorizationResponse_With_AuthorizationCode_Then_Code_Is_Added_To_The_Parameters()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "openid";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = scope
            };
            var consent = new Consent();
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var jwsPayload = new JwsPayload();
            _parameterParserHelperFake.Setup(p => p.ParseResponseType(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.code  
                });
            _jwtGeneratorFake.Setup(
                j => j.GenerateIdTokenPayloadForScopes(It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthorizationParameter>()))
                .Returns(jwsPayload);
            _jwtGeneratorFake.Setup(
                j => j.GenerateUserInfoPayloadForScope(It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthorizationParameter>()))
                .Returns(jwsPayload);
            _jwtGeneratorFake.Setup(j => j.Encrypt(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(idToken);
            _consentHelperFake.Setup(c => c.GetConsentConfirmedByResourceOwner(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(consent);

            // ACT
            _generateAuthorizationResponse.Execute(actionResult, authorizationParameter, claimsPrincipal);

            // ASSERTS
            Assert.IsTrue(actionResult.RedirectInstruction.Parameters.Any(p => p.Name == Core.Constants.StandardAuthorizationResponseNames.AuthorizationCodeName));
            _authorizationCodeRepositoryFake.Verify(a => a.AddAuthorizationCode(It.IsAny<AuthorizationCode>()));
            _simpleIdentityServerEventSource.Verify(s => s.GrantAuthorizationCodeToClient(clientId, It.IsAny<string>(), scope));
        }

        [Test]
        public void When_An_Authorization_Response_Is_Generated_Then_Events_Are_Logged()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "scope";
            const string responseType = "id_token";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = scope,
                ResponseType = responseType
            };
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var jwsPayload = new JwsPayload();
            _parameterParserHelperFake.Setup(p => p.ParseResponseType(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.id_token  
                });
            _jwtGeneratorFake.Setup(
                j => j.GenerateIdTokenPayloadForScopes(It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthorizationParameter>()))
                .Returns(jwsPayload);
            _jwtGeneratorFake.Setup(
                j => j.GenerateUserInfoPayloadForScope(It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthorizationParameter>()))
                .Returns(jwsPayload);
            _jwtGeneratorFake.Setup(j => j.Encrypt(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(idToken);

            // ACT
            _generateAuthorizationResponse.Execute(actionResult, authorizationParameter, claimsPrincipal);

            // ASSERT
            _simpleIdentityServerEventSource.Verify(s => s.StartGeneratingAuthorizationResponseToClient(clientId, responseType));
            _simpleIdentityServerEventSource.Verify(s => s.EndGeneratingAuthorizationResponseToClient(clientId, actionResult.RedirectInstruction.Parameters.SerializeWithJavascript()));
        }

        [Test]
        public void When_Redirecting_To_Callback_And_There_Is_No_Response_Mode_Specified_Then_The_Response_Mode_Is_Set()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "scope";
            const string responseType = "id_token";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = scope,
                ResponseType = responseType,
                ResponseMode = ResponseMode.None
            };
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction(),
                Type = TypeActionResult.RedirectToCallBackUrl
            };
            var jwsPayload = new JwsPayload();
            _parameterParserHelperFake.Setup(p => p.ParseResponseType(It.IsAny<string>()))
                .Returns(new List<ResponseType>
                {
                    ResponseType.id_token  
                });
            _jwtGeneratorFake.Setup(
                j => j.GenerateIdTokenPayloadForScopes(It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthorizationParameter>()))
                .Returns(jwsPayload);
            _jwtGeneratorFake.Setup(
                j => j.GenerateUserInfoPayloadForScope(It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthorizationParameter>()))
                .Returns(jwsPayload);
            _jwtGeneratorFake.Setup(j => j.Encrypt(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(idToken);
            _authorizationFlowHelperFake.Setup(
                a => a.GetAuthorizationFlow(It.IsAny<ICollection<ResponseType>>(), It.IsAny<string>()))
                .Returns(AuthorizationFlow.ImplicitFlow);

            // ACT
            _generateAuthorizationResponse.Execute(actionResult, authorizationParameter, claimsPrincipal);

            // ASSERT
            Assert.IsTrue(actionResult.RedirectInstruction.ResponseMode == ResponseMode.fragment);
        }

        private void InitializeFakeObjects()
        {
            _authorizationCodeRepositoryFake = new Mock<IAuthorizationCodeRepository>();
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _jwtGeneratorFake = new Mock<IJwtGenerator>();
            _grantedTokenGeneratorHelperFake = new Mock<IGrantedTokenGeneratorHelper>();
            _grantedTokenRepositoryFake = new Mock<IGrantedTokenRepository>();
            _consentHelperFake = new Mock<IConsentHelper>();
            _simpleIdentityServerEventSource = new Mock<ISimpleIdentityServerEventSource>();
            _authorizationFlowHelperFake = new Mock<IAuthorizationFlowHelper>();
            _generateAuthorizationResponse = new GenerateAuthorizationResponse(
                _authorizationCodeRepositoryFake.Object,
                _parameterParserHelperFake.Object,
                _jwtGeneratorFake.Object,
                _grantedTokenGeneratorHelperFake.Object,
                _grantedTokenRepositoryFake.Object,
                _consentHelperFake.Object,
                _simpleIdentityServerEventSource.Object,
                _authorizationFlowHelperFake.Object);
        }
    }
}
