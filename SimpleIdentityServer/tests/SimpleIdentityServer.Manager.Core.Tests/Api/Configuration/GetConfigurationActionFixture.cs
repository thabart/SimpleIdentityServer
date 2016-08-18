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
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Manager.Core.Api.Configuration.Actions;
using System;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Configuration
{
    public class GetConfigurationActionFixture
    {
        #region Fields

        private Mock<IConfigurationRepository> _configurationRepositoryStub;

        private IGetConfigurationAction _getConfigurationAction;

        #endregion

        #region Exceptions

        [Fact]
        public void When_Passing_NullOr_Empty_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _getConfigurationAction.Execute(null));
            Assert.Throws<ArgumentNullException>(() => _getConfigurationAction.Execute(string.Empty));
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_Configuration_Is_Gotten_Then_Operation_Is_Called()
        {
            // ARRANGE
            const string key = "configuration_key";
            InitializeFakeObjects();

            // ACT
            _getConfigurationAction.Execute(key);

            // ASSERT
            _configurationRepositoryStub.Verify(c => c.Get(key));
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _configurationRepositoryStub = new Mock<IConfigurationRepository>();
            _getConfigurationAction = new GetConfigurationAction(_configurationRepositoryStub.Object);
        }

        #endregion

    }
}
