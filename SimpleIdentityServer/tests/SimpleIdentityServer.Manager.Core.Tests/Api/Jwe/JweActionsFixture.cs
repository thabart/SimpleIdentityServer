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
using SimpleIdentityServer.Manager.Core.Api.Jwe;
using SimpleIdentityServer.Manager.Core.Api.Jwe.Actions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Jwe
{
    public class JweActionsFixture
    {
        private Mock<IGetJweInformationAction> _getJweInformationActionStub;

        private Mock<ICreateJweAction> _createJweActionStub;

        private IJweActions _jweActions;

        #region Exceptions GetJweInformation

        [Fact]
        public void When_Passing_Null_Parameter_To_GetJweInformation_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.ThrowsAsync<ArgumentNullException>(() => _jweActions.GetJweInformation(null));
        }

        #endregion

        #region Happy path GetJweInformation 

        [Fact]
        public async Task When_Execute_GetJweInformation_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new Parameters.GetJweParameter();

            // ACT 
            await _jweActions.GetJweInformation(parameter).ConfigureAwait(false);

            // ASSERT
            _getJweInformationActionStub.Verify(g => g.ExecuteAsync(parameter));
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _getJweInformationActionStub = new Mock<IGetJweInformationAction>();
            _createJweActionStub = new Mock<ICreateJweAction>();
            _jweActions = new JweActions(
                _getJweInformationActionStub.Object,
                _createJweActionStub.Object);
        }
    }
}
