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
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Stores;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using SimpleIdentityServer.Core.Common;

namespace SimpleIdentityServer.Core.UnitTests.Api.Introspection.Actions
{
    public class PostIntrospectionActionFixture
    {
        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceStub;
        private Mock<IAuthenticateClient> _authenticateClientStub;
        private Mock<IIntrospectionParameterValidator> _introspectionParameterValidatorStub;
        private Mock<ITokenStore> _tokenStoreStub;
        private IPostIntrospectionAction _postIntrospectionAction;

        #region Exceptions

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _postIntrospectionAction.Execute(null, null));
        }

        [Fact]
        public async Task When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new IntrospectionParameter
            {
                Token = "token"
            };
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>()))
               .Returns(Task.FromResult(new AuthenticationResult(null, null)));

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _postIntrospectionAction.Execute(parameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidClient);
        }
        
        [Fact]
        public async Task When_AccessToken_Cannot_Be_Extracted_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new IntrospectionParameter
            {
                TokenTypeHint = Constants.StandardTokenTypeHintNames.AccessToken,
                Token = "token"
            };
            var client = new AuthenticationResult(new Client(), null);
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>()))
                .Returns(Task.FromResult(client));
            _tokenStoreStub.Setup(a => a.GetAccessToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));
            _tokenStoreStub.Setup(a => a.GetRefreshToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _postIntrospectionAction.Execute(parameter, null));
            Assert.True(exception.Code == ErrorCodes.InvalidToken);
            Assert.True(exception.Message == ErrorDescriptions.TheTokenIsNotValid);
        }
        
        #endregion

        #region Happy paths

        [Fact]
        public async Task When_Passing_Expired_AccessToken_Then_Result_Should_Be_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            const string subject = "subject";
            const string audience = "audience";
            var authenticationHeaderValue = new AuthenticationHeaderValue("Basic", "ClientId:ClientSecret".Base64Encode());
            var audiences = new[]
            {
                audience
            };
            var parameter = new IntrospectionParameter
            {
                TokenTypeHint = Constants.StandardTokenTypeHintNames.RefreshToken,
                Token = "token"
            };
            var client = new AuthenticationResult(new Client
            {
                ClientId = clientId
            }, null);
            var grantedToken = new GrantedToken
            {
                ClientId = clientId,
                IdTokenPayLoad = new JwsPayload
                {
                    {
                        Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                        subject
                    },
                    {
                        StandardClaimNames.Audiences,
                        audiences
                    }
                },
                CreateDateTime = DateTime.UtcNow.AddDays(-2),
                ExpiresIn = 2
            };
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>()))
                .Returns(() => Task.FromResult(client));
            _tokenStoreStub.Setup(a => a.GetRefreshToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));
            _tokenStoreStub.Setup(a => a.GetAccessToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult(grantedToken));

            // ACT
            var result = await _postIntrospectionAction.Execute(parameter, authenticationHeaderValue);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.Active);
            Assert.True(result.Audience == audience);
            Assert.True(result.Subject == subject);
        }

        [Fact]
        public async Task When_Passing_Active_AccessToken_Then_Result_Should_Be_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            const string subject = "subject";
            const string audience = "audience";
            var authenticationHeaderValue = new AuthenticationHeaderValue("Basic", "ClientId:ClientSecret".Base64Encode());
            var audiences = new[]
            {
                audience
            };
            var parameter = new IntrospectionParameter
            {
                TokenTypeHint = Constants.StandardTokenTypeHintNames.RefreshToken,
                Token = "token"
            };
            var client = new AuthenticationResult(new Client
            {
                ClientId = clientId
            }, null);
            var grantedToken = new GrantedToken
            {
                ClientId = clientId,
                IdTokenPayLoad = new JwsPayload
                {
                    {
                        Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                        subject
                    },
                    {
                        StandardClaimNames.Audiences,
                        audiences
                    }
                },
                CreateDateTime = DateTime.UtcNow,
                ExpiresIn = 20000
            };
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>()))
                .Returns(Task.FromResult(client));
            _tokenStoreStub.Setup(a => a.GetRefreshToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));
            _tokenStoreStub.Setup(a => a.GetAccessToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult(grantedToken));

            // ACT
            var result = await _postIntrospectionAction.Execute(parameter, authenticationHeaderValue);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Active);
            Assert.True(result.Audience == audience);
            Assert.True(result.Subject == subject);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _simpleIdentityServerEventSourceStub = new Mock<ISimpleIdentityServerEventSource>();
            _authenticateClientStub = new Mock<IAuthenticateClient>();
            _introspectionParameterValidatorStub = new Mock<IIntrospectionParameterValidator>();
            _tokenStoreStub = new Mock<ITokenStore>();
            _postIntrospectionAction = new PostIntrospectionAction(
                _simpleIdentityServerEventSourceStub.Object,
                _authenticateClientStub.Object,
                _introspectionParameterValidatorStub.Object,
                _tokenStoreStub.Object);
        }
    }
}
