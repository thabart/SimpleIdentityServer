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
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Manager.Core.Api.Clients.Actions;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using System;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Clients.Actions
{
    public class GetClientActionFixture
    {
        private Mock<IClientRepository> _clientRepositoryStub;

        private IGetClientAction _getClientAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _getClientAction.Execute(null));
        }

        [Fact]
        public void When_Client_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string clientId = "client_id";
            InitializeFakeObjects();
            _clientRepositoryStub.Setup(c => c.GetClientById(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerManagerException>(() => _getClientAction.Execute(clientId));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClientDoesntExist, clientId));
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_Getting_Client_Then_Information_Are_Returned()
        {
            // ARRANGE
            const string clientId = "clientId";
            var client = new SimpleIdentityServer.Core.Models.Client
            {
                ClientId = clientId
            };
            InitializeFakeObjects();
            _clientRepositoryStub.Setup(c => c.GetClientById(It.IsAny<string>()))
                .Returns(client);

            // ACT
            var result = _getClientAction.Execute(clientId);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.ClientId == clientId);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _clientRepositoryStub = new Mock<IClientRepository>();
            _getClientAction = new GetClientAction(_clientRepositoryStub.Object);
        }
    }
}
