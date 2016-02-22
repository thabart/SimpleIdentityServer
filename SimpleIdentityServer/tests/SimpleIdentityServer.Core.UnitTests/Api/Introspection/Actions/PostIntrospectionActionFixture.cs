#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Moq;
using SimpleIdentityServer.Core.Api.Introspection.Actions;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Introspection.Actions
{
    public class PostIntrospectionActionFixture
    {
        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceStub;

        private Mock<IAuthenticateClient> _authenticateClientStub;

        private Mock<IIntrospectionParameterValidator> _introspectionParameterValidatorStub;

        private Mock<IGrantedTokenRepository> _grantedTokenRepositoryStub;

        private IPostIntrospectionAction _postIntrospectionAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _postIntrospectionAction.Execute(null, null));
        }

        [Fact]
        public void When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new IntrospectionParameter();
            string errorMessage;
            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), out errorMessage))
                .Returns(() => null);

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerException>(() => _postIntrospectionAction.Execute(parameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidClient);
        }
        
        [Fact]
        public void When_AccessToken_Cannot_Be_Extracted_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new IntrospectionParameter
            {
                TokenTypeHint = Constants.StandardTokenTypeHintNames.AccessToken,
                Token = "token"
            };
            string errorMessage;
            var client = new Client();
            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), out errorMessage))
                .Returns(client);
            _grantedTokenRepositoryStub.Setup(a => a.GetToken(It.IsAny<string>()))
                .Returns(() => null);
            _grantedTokenRepositoryStub.Setup(a => a.GetTokenByRefreshToken(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => _postIntrospectionAction.Execute(parameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidToken);
            Assert.True(exception.Message == ErrorDescriptions.TheTokenIsNotValid);
        }

        [Fact]
        public void When_Token_Has_Not_Been_Issued_By_The_Same_Client_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string fakeClientId = "fake_client_id";
            var parameter = new IntrospectionParameter
            {
                TokenTypeHint = Constants.StandardTokenTypeHintNames.RefreshToken,
                Token = "token"
            };
            string errorMessage;
            var client = new Client
            {
                ClientId = fakeClientId
            };
            var grantedToken = new GrantedToken
            {
                ClientId = "client_id"
            };
            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), out errorMessage))
                .Returns(client);
            _grantedTokenRepositoryStub.Setup(a => a.GetTokenByRefreshToken(It.IsAny<string>()))
                .Returns(() => null);
            _grantedTokenRepositoryStub.Setup(a => a.GetToken(It.IsAny<string>()))
                .Returns(grantedToken);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => _postIntrospectionAction.Execute(parameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidToken);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheTokenHasNotBeenIssuedForTheGivenClientId, fakeClientId));
        }

        #endregion

        #region Happy paths

        [Fact]
        public void When_Passing_Valid_AccessToken_Then_Result_Should_Be_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            const string subject = "subject";
            const string audience = "audience";
            var audiences = new[]
            {
                audience
            };
            var parameter = new IntrospectionParameter
            {
                TokenTypeHint = Constants.StandardTokenTypeHintNames.RefreshToken,
                Token = "token"
            };
            string errorMessage;
            var client = new Client
            {
                ClientId = clientId
            };
            var grantedToken = new GrantedToken
            {
                ClientId = clientId,
                UserInfoPayLoad = new Jwt.JwsPayload
                {
                    {
                        Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                        subject
                    },
                    {
                        Jwt.Constants.StandardClaimNames.Audiences,
                        audiences
                    }
                },
                CreateDateTime = DateTime.UtcNow.AddDays(-2),
                ExpiresIn = 2
            };
            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), out errorMessage))
                .Returns(client);
            _grantedTokenRepositoryStub.Setup(a => a.GetTokenByRefreshToken(It.IsAny<string>()))
                .Returns(() => null);
            _grantedTokenRepositoryStub.Setup(a => a.GetToken(It.IsAny<string>()))
                .Returns(grantedToken);

            // ACT
            var result = _postIntrospectionAction.Execute(parameter, null);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.Active);
            Assert.True(result.Audience == audience);
            Assert.True(result.Subject == subject);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _simpleIdentityServerEventSourceStub = new Mock<ISimpleIdentityServerEventSource>();
            _authenticateClientStub = new Mock<IAuthenticateClient>();
            _introspectionParameterValidatorStub = new Mock<IIntrospectionParameterValidator>();
            _grantedTokenRepositoryStub = new Mock<IGrantedTokenRepository>();
            _postIntrospectionAction = new PostIntrospectionAction(
                _simpleIdentityServerEventSourceStub.Object,
                _authenticateClientStub.Object,
                _introspectionParameterValidatorStub.Object,
                _grantedTokenRepositoryStub.Object);
        }
    }
}
