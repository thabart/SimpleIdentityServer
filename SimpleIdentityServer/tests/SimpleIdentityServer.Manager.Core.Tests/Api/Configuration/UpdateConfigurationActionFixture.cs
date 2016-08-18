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
using SimpleIdentityServer.Manager.Core.Parameters;
using System;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Configuration
{
    public class UpdateConfigurationActionFixture
    {
        #region Fields

        private Mock<IConfigurationRepository> _configurationRepositoryStub;

        private IUpdateConfigurationAction _updateConfigurationAction;

        #endregion

        #region Exceptions

        [Fact]
        public void When_Passing_NullOr_Empty_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _updateConfigurationAction.Execute(null));
            Assert.Throws<ArgumentNullException>(() => _updateConfigurationAction.Execute(new UpdateConfigurationParameter()));
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_Configuration_Is_Updated_Then_Operation_Is_Called()
        {
            // ARRANGE
            const string key = "configuration_key";
            InitializeFakeObjects();

            // ACT
            _updateConfigurationAction.Execute(new UpdateConfigurationParameter
            {
                Key = key
            });

            // ASSERT
            _configurationRepositoryStub.Verify(c => c.Update(It.IsAny<SimpleIdentityServer.Core.Models.Configuration>()));
        }

        #endregion


        #region Private methods

        private void InitializeFakeObjects()
        {
            _configurationRepositoryStub = new Mock<IConfigurationRepository>();
            _updateConfigurationAction = new UpdateConfigurationAction(_configurationRepositoryStub.Object);
        }

        #endregion
    }
}
