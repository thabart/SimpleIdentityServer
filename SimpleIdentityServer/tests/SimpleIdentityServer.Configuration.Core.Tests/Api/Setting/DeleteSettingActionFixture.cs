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
using SimpleIdentityServer.Configuration.Core.Repositories;
using SimpleIdentityServer.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Configuration.Core.Tests.Api.Setting
{
    public class DeleteSettingActionFixture
    {
        private Mock<ISettingRepository> _settingRepositoryStub;
        private Mock<IConfigurationEventSource> _configurationEventSource;
        private IDeleteSettingAction _deleteSettingAction;

        [Fact]
        public async Task When_Passing_NullOr_Empty_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _deleteSettingAction.Execute(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _deleteSettingAction.Execute(string.Empty));
        }

        [Fact]
        public async Task When_Setting_Is_Removed_Then_Operation_Is_Called()
        {
            // ARRANGE
            const string key = "configuration_key";
            InitializeFakeObjects();

            // ACT
            await _deleteSettingAction.Execute(key);

            // ASSERT
            _settingRepositoryStub.Verify(c => c.Remove(key));
        }

        private void InitializeFakeObjects()
        {
            _settingRepositoryStub = new Mock<ISettingRepository>();
            _configurationEventSource = new Mock<IConfigurationEventSource>();
            _deleteSettingAction = new DeleteSettingAction(
                _settingRepositoryStub.Object,
                _configurationEventSource.Object);
        }
    }
}
