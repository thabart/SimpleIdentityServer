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
using SimpleIdentityServer.Configuration.Core.Api.Setting.Actions;
using SimpleIdentityServer.Configuration.Core.Parameters;
using SimpleIdentityServer.Configuration.Core.Repositories;
using System;
using Xunit;

namespace SimpleIdentityServer.Configuration.Core.Tests.Api.Setting
{
    public class UpdateSettingActionFixture
    {
        #region Fields

        private Mock<ISettingRepository> _settingRepositoryStub;

        private IUpdateSettingAction _updateSettingAction;

        #endregion

        #region Exceptions

        [Fact]
        public void When_Passing_NullOr_Empty_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _updateSettingAction.Execute(null));
            Assert.Throws<ArgumentNullException>(() => _updateSettingAction.Execute(new UpdateSettingParameter()));
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_Setting_Is_Updated_Then_Operation_Is_Called()
        {
            // ARRANGE
            const string key = "configuration_key";
            InitializeFakeObjects();

            // ACT
            _updateSettingAction.Execute(new UpdateSettingParameter
            {
                Key = key
            });

            // ASSERT
            _settingRepositoryStub.Verify(c => c.Update(It.IsAny<Models.Setting>()));
        }

        #endregion


        #region Private methods

        private void InitializeFakeObjects()
        {
            _settingRepositoryStub = new Mock<ISettingRepository>();
            _updateSettingAction = new UpdateSettingAction(_settingRepositoryStub.Object);
        }

        #endregion
    }
}
