#region copyright
// Copyright 2016 Habart Thierry
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
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Client.Builders;
using SimpleIdentityServer.Scim.Client.Factories;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Scim.Client.Tests
{
    public class UsersClientFixture : IClassFixture<TestScimServerFixture>
    {
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private readonly TestScimServerFixture _testScimServerFixture;
        private IUsersClient _usersClient;

        public UsersClientFixture(TestScimServerFixture testScimServerFixture)
        {
            _testScimServerFixture = testScimServerFixture;
        }

        [Fact]
        public async Task When_Executing_Operations_On_Users_Then_No_Exceptions_Are_Thrown()
        {
            const string baseUrl = "http://localhost:5555";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testScimServerFixture.Client);
            var patchOperation = new PatchOperationBuilder().SetType(PatchOperations.replace)
                .SetPath(Common.Constants.UserResourceResponseNames.UserName)
                .SetContent("new_username")
                .Build();

            // ACT : Create user
            var firstResult = await _usersClient.AddUser(baseUrl)
                .SetCommonAttributes("external_id")
                .AddAttribute(new JProperty(Common.Constants.UserResourceResponseNames.UserName, "username"))
                .Execute();

            // ASSERTS
            Assert.NotNull(firstResult);
            Assert.True(firstResult.StatusCode == HttpStatusCode.Created);
            var id = firstResult.Content["id"].ToString();

            // ACT : Partial update user
            var secondResult = await _usersClient.PartialUpdateUser(baseUrl, id)
                .AddOperation(patchOperation)
                .Execute();

            // ASSERTS
            Assert.NotNull(secondResult);
            Assert.True(secondResult.Content[Common.Constants.UserResourceResponseNames.UserName].ToString() == "new_username");

            // ACT : Update user
            var thirdResult = await _usersClient.UpdateUser(baseUrl, id)
                .SetCommonAttributes("new_external_id")
                .AddAttribute(new JProperty(Common.Constants.UserResourceResponseNames.UserName, "other_username"))
                .AddAttribute(new JProperty(Common.Constants.UserResourceResponseNames.Active, "false"))
                .Execute();

            // ASSERTS
            Assert.NotNull(thirdResult);
            Assert.True(thirdResult.StatusCode == HttpStatusCode.OK);
            Assert.True(thirdResult.Content[Common.Constants.UserResourceResponseNames.UserName].ToString() == "other_username");
            var active = thirdResult.Content[Common.Constants.UserResourceResponseNames.Active].ToString();
            Assert.False(bool.Parse(active));
            Assert.True(thirdResult.Content[Common.Constants.IdentifiedScimResourceNames.ExternalId].ToString() == "new_external_id");
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _usersClient = new UsersClient(_httpClientFactoryStub.Object);
        }
    }
}
