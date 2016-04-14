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
using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System;
using System.Net.Http.Headers;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Token
{
    public class GetTokenByClientCredentialsGrantTypeActionFixture
    {
        private Mock<IAuthenticateInstructionGenerator> _authenticateInstructionGeneratorStub;

        private Mock<IAuthenticateClient> _authenticateClientStub;

        private Mock<IClientValidator> _clientValidatorStub;

        private Mock<IGrantedTokenGeneratorHelper> _grantedTokenGeneratorHelperStub;

        private Mock<IScopeValidator> _scopeValidatorStub;

        private Mock<IGrantedTokenRepository> _grantedTokenRepositoryStub;

        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceStub;

        private IGetTokenByClientCredentialsGrantTypeAction _getTokenByClientCredentialsGrantTypeAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(null, null));
        }

        [Fact]
        public void When_Passing_Empty_Socpe_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter();

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.ScopeName));
        }

        [Fact]
        public void When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var message = "message";
            InitializeFakeObjects();
            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
            {
                Scope = "scope"
            };
            var authenticateInstruction = new AuthenticateInstruction();
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(authenticateInstruction);
            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), out message))
                .Returns(() => null);

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidClient);
            Assert.True(exception.Message == message);
        }

        #endregion


        #region Happy paths



        #endregion

        private void InitializeFakeObjects()
        {
            _authenticateInstructionGeneratorStub = new Mock<IAuthenticateInstructionGenerator>();
            _authenticateClientStub = new Mock<IAuthenticateClient>();
            _clientValidatorStub = new Mock<IClientValidator>();
            _grantedTokenGeneratorHelperStub = new Mock<IGrantedTokenGeneratorHelper>();
            _scopeValidatorStub = new Mock<IScopeValidator>();
            _grantedTokenRepositoryStub = new Mock<IGrantedTokenRepository>();
            _simpleIdentityServerEventSourceStub = new Mock<ISimpleIdentityServerEventSource>();
            _getTokenByClientCredentialsGrantTypeAction = new GetTokenByClientCredentialsGrantTypeAction(
                _authenticateInstructionGeneratorStub.Object,
                _authenticateClientStub.Object,
                _clientValidatorStub.Object,
                _grantedTokenGeneratorHelperStub.Object,
                _scopeValidatorStub.Object,
                _grantedTokenRepositoryStub.Object,
                _simpleIdentityServerEventSourceStub.Object);
        }
    }
}
