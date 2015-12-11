using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Moq;
using NUnit.Framework;
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

namespace SimpleIdentityServer.Api.UnitTests.Common
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
        public void When_Generating_AuthorizationResponse_With_IdToken_Then_IdToken_Is_Added_To_The_Parameters()
        {
            // ARRANGE
            InitializeFakeObjects();
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
            _generateAuthorizationResponse.Execute(actionResult, authorizationParameter, null);

            // ASSERT
            Assert.IsTrue(actionResult.RedirectInstruction.Parameters.Any(p => p.Name == Core.Constants.StandardAuthorizationResponseNames.IdTokenName));
            Assert.IsTrue(actionResult.RedirectInstruction.Parameters.Any(p => p.Value == idToken));
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
            _generateAuthorizationResponse.Execute(actionResult, authorizationParameter, null);

            // ASSERT
            _simpleIdentityServerEventSource.Verify(s => s.StartGeneratingAuthorizationResponseToClient(clientId, responseType));
            _simpleIdentityServerEventSource.Verify(s => s.EndGeneratingAuthorizationResponseToClient(clientId, actionResult.RedirectInstruction.Parameters.SerializeWithJavascript()));
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
            _generateAuthorizationResponse = new GenerateAuthorizationResponse(
                _authorizationCodeRepositoryFake.Object,
                _parameterParserHelperFake.Object,
                _jwtGeneratorFake.Object,
                _grantedTokenGeneratorHelperFake.Object,
                _grantedTokenRepositoryFake.Object,
                _consentHelperFake.Object,
                _simpleIdentityServerEventSource.Object);
        }
    }
}
