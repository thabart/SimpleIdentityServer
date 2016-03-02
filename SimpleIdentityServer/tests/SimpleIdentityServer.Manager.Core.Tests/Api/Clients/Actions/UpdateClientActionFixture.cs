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
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Manager.Core.Api.Clients.Actions;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Parameters;
using System;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Clients.Actions
{
    public class UpdateClientActionFixture
    {
        private Mock<IClientRepository> _clientRepositoryStub;

        private Mock<IGenerateClientFromRegistrationRequest> _generateClientFromRegistrationRequestStub;

        private IUpdateClientAction _updateClientAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _updateClientAction.Execute(null));
        }

        [Fact]
        public void When_Client_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string clientId = "invalid_client_id";
            InitializeFakeObjects();
            _clientRepositoryStub.Setup(c => c.GetClientById(It.IsAny<string>()))
                .Returns(() => null);
            var parameter = new UpdateClientParameter
            {
                ClientId = clientId
            };

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerManagerException>(() => _updateClientAction.Execute(parameter));
            Assert.True(exception.Code == ErrorCodes.InvalidParameterCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClientDoesntExist, clientId));
        }

        [Fact]
        public void When_An_Exception_Is_Raised_While_Attempting_To_Create_A_Client_Then_Exception_Is_Thrown()  
        {
            // ARRANGE
            const string clientId = "client_id";
            const string code = "code";
            const string message = "message";
            var client = new Client
            {
                ClientId = clientId
            };
            var parameter = new UpdateClientParameter
            {
                ClientId = clientId
            };
            InitializeFakeObjects();
            _clientRepositoryStub.Setup(c => c.GetClientById(It.IsAny<string>()))
                .Returns(client);
            _generateClientFromRegistrationRequestStub.Setup(g => g.Execute(It.IsAny<UpdateClientParameter>()))
                .Callback(() =>
                {
                    throw new IdentityServerException(code, message);
                });

            // ACT & ASSERT
            var exception = Assert.Throws<IdentityServerManagerException>(() => _updateClientAction.Execute(parameter));
            Assert.True(exception.Code == code);
            Assert.True(exception.Message == message);
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_Passing_Correct_Parameter_Then_Update_Operation_Is_Called()
        {
            // ARRANGE
            const string clientId = "client_id";
            const string code = "code";
            const string message = "message";
            var client = new Client
            {
                ClientId = clientId
            };
            var parameter = new UpdateClientParameter
            {
                ClientId = clientId
            };
            InitializeFakeObjects();
            _clientRepositoryStub.Setup(c => c.GetClientById(It.IsAny<string>()))
                .Returns(client);
            _generateClientFromRegistrationRequestStub.Setup(g => g.Execute(It.IsAny<UpdateClientParameter>()))
                .Returns(client);

            // ACT
            _updateClientAction.Execute(parameter);

            // ASSERTS
            _clientRepositoryStub.Verify(c => c.UpdateClient(client));
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _clientRepositoryStub = new Mock<IClientRepository>();
            _generateClientFromRegistrationRequestStub = new Mock<IGenerateClientFromRegistrationRequest>();
            _updateClientAction = new UpdateClientAction(
                _clientRepositoryStub.Object,
                _generateClientFromRegistrationRequestStub.Object);
        }
    }
}
