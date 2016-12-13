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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Configuration.Core.Tests.Api.Setting
{
    public class BulkGetSettingsOperationFixture
    {
        private Mock<ISettingRepository> _settingRepositoryStub;
        private IBulkGetSettingsOperation _bulkGetSettingsOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _bulkGetSettingsOperation.Execute(null));
        }

        [Fact]
        public async Task When_GetSettings_Then_Operation_Is_Called()
        {
            // ARRANGE
            var parameter = new Parameters.GetBulkSettingsParameter
            {
                Ids = new List<string>
                {
                    "id"
                }
            };
            InitializeFakeObjects();

            // ACT
            await _bulkGetSettingsOperation.Execute(parameter);

            // ASSERT
            _settingRepositoryStub.Verify(s => s.Get(parameter.Ids));
        }

        private void InitializeFakeObjects()
        {
            _settingRepositoryStub = new Mock<ISettingRepository>();
            _bulkGetSettingsOperation = new BulkGetSettingsOperation(_settingRepositoryStub.Object);
        }
    }
}
