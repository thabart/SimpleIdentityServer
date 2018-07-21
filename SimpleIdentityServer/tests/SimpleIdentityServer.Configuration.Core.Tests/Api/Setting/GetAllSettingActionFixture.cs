﻿#region copyright
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
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Configuration.Core.Tests.Api.Setting
{
    public class GetAllSettingActionFixture
    {
        private Mock<ISettingRepository> _settingRepositoryStub;
        private IGetAllSettingAction _getAllSettingAction;

        [Fact]
        public async Task When_Configurations_Are_Retrieved_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            await _getAllSettingAction.Execute().ConfigureAwait(false);

            // ASSERT
            _settingRepositoryStub.Verify(c => c.GetAll());
        }
        
        private void InitializeFakeObjects()
        {
            _settingRepositoryStub = new Mock<ISettingRepository>();
            _getAllSettingAction = new GetAllSettingAction(_settingRepositoryStub.Object);
        }
    }
}
